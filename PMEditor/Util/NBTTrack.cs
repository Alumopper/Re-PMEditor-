using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpNBT;
using Expression = NCalc.Expression;

namespace PMEditor.Util;

internal struct Range
{
    public int start;
    public int end;
    public double startPos;
    public double length = 0;

    public Range(int start, int end, double startPos)
    {
        this.start = start;
        this.end = end;
        this.startPos = startPos;
    }
}

public class NBTTrack
{
    public double bpm; //bpm
    public double count; //总物量
    public string difficulty = ""; //谱面难度
    public double length; //曲目长度
    public NBTLine[] lines = Array.Empty<NBTLine>();
    public string musicAuthor = ""; //曲师
    private Track originalTrack;
    public string trackAuthor = ""; //谱师

    public string trackName = ""; //谱面名字

    public CompoundTag ToNBTTag()
    {
        CompoundTag trackNBT = new("root")
        {
            new StringTag("trackName", trackName),
            new StringTag("musicAuthor", musicAuthor),
            new StringTag("trackAuthor", trackAuthor),
            new DoubleTag("bpm", bpm),
            new DoubleTag("length", length),
            new StringTag("difficulty", difficulty),
            new IntTag("tickLength", (int)(length * Settings.currSetting.Tick)),
            new DoubleTag("count", count)
        };
        ListTag lineNBT = new("lines", TagType.Compound);
        foreach (var line in lines)
        {
            CompoundTag compound = new("line");
            //notes
            ListTag notesNBT = new("notes", TagType.Compound);
            foreach (var note in line.notes)
            {
                CompoundTag noteNBT;
                if (note is NBTHold hold)
                    noteNBT = new CompoundTag("hold")
                    {
                        new IntTag("summonTick", (int)(hold.summonTime * Settings.currSetting.Tick)),
                        new IntTag("judgeTick", (int)(hold.judgeTime * Settings.currSetting.Tick)),
                        new DoubleTag("summonPos", hold.summonPos),
                        new ByteTag("isFake", hold.isFake),
                        new DoubleTag("holdTime", hold.holdTime),
                        new DoubleTag("holdLength", hold.holdLength)
                    };
                else
                    noteNBT = new CompoundTag("note")
                    {
                        new IntTag("summonTick", (int)(note.summonTime * Settings.currSetting.Tick)),
                        new IntTag("judgeTick", (int)(note.judgeTime * Settings.currSetting.Tick)),
                        new DoubleTag("summonPos", note.summonPos),
                        new ByteTag("isFake", note.isFake),
                        new ByteTag("type", note.type)
                    };
                notesNBT.Add(noteNBT);
            }

            compound.Add(notesNBT);
            lineNBT.Add(compound);
            //速度
            ListTag speedNBT = new("speeds", TagType.Double);
            foreach (var speed in line.speedList) speedNBT.Add(new DoubleTag("speed", speed));
            compound.Add(speedNBT);
        }

        trackNBT.Add(lineNBT);
        return trackNBT;
    }

    public static NBTTrack FromTrack(Track track)
    {
        NBTTrack nbtTrack = new()
        {
            //基本信息
            trackName = track.TrackName,
            musicAuthor = track.MusicAuthor,
            trackAuthor = track.TrackAuthor,
            bpm = track.Bpm,
            length = track.Length,
            difficulty = track.Difficulty,
            count = track.Count,
            originalTrack = track
        };

        //判定线信息
        List<NBTLine> lines = new();
        foreach (var line in track.AllLines)
        {
            NBTLine nbtLine = new()
            {
                line = line
            };
            //notes
            List<NBTNote> notes = new();
            foreach (var note in line.Notes)
            {
                var ranges = AppearTickRange(note);
                if (note.type == NoteType.Hold)
                {
                    //hold
                    if (ranges.Count == 0)
                    {
                        //一直在屏幕外面
                        notes.Add(
                            new NBTHold(
                                new Range(
                                    (int)(note.ActualTime * Settings.currSetting.Tick),
                                    (int)(note.ActualTime * Settings.currSetting.Tick),
                                    0
                                ),
                                0,
                                (int)(note.ActualHoldTime * Settings.currSetting.Tick),
                                note.Rail
                            ).Apply(it =>
                            {
                                it.Expression = note.Expression;
                            })
                        );
                    }
                    else
                    {
                        notes.Add(
                            new NBTHold(
                                ranges[0],
                                0,
                                (int)(note.ActualHoldTime * Settings.currSetting.Tick)
                                , note.Rail
                            ).Apply(it =>
                            {
                                it.Expression = note.Expression;
                            })
                        );
                        for (var i = 1; i < ranges.Count; i++)
                            notes.Add(
                                new NBTHold(
                                    ranges[i],
                                    1,
                                    (int)(note.ActualHoldTime * Settings.currSetting.Tick),
                                    note.Rail).Apply(it =>
                                {
                                    it.Expression = note.Expression;
                                })
                            );
                    }
                }
                else
                {
                    //不是hold
                    if (ranges.Count == 0)
                    {
                        notes.Add(
                            new NBTNote(
                                new Range(
                                    (int)(note.ActualTime * Settings.currSetting.Tick),
                                    (int)(note.ActualTime * Settings.currSetting.Tick),
                                    0
                                ),
                                (byte)note.NoteType,
                                note.Rail
                            ).Apply(it =>
                            {
                                it.Expression = note.Expression;
                            })
                        );
                    }
                    else
                    {
                        notes.Add(new NBTNote(ranges[0], (byte)note.NoteType, note.Rail).Apply(it =>
                        {
                            it.Expression = note.Expression;
                        }));
                        for (var i = 1; i < ranges.Count; i++)
                            notes.Add(new NBTNote(ranges[i], (byte)note.NoteType, note.Rail, 1).Apply(it =>
                            {
                                it.Expression = note.Expression;
                            }));
                    }
                }
            }

            //fake catch
            foreach (var f in line.FakeCatch)
            {
                var ranges = AppearTickRange(f);
                if (ranges.Count == 0)
                {
                    notes.Add(new NBTFakeCatch(
                        new Range((int)(f.ActualTime * Settings.currSetting.Tick),
                            (int)(f.ActualTime * Settings.currSetting.Tick), 0), f.Rail, f.Height).Apply(it =>
                    {
                        it.Expression = f.Expression;
                    }));
                }
                else
                {
                    notes.Add(new NBTFakeCatch(ranges[0], f.Rail, f.Height).Apply(it =>
                    {
                        it.Expression = f.Expression;
                    }));
                    for (var i = 1; i < ranges.Count; i++)
                        notes.Add(new NBTFakeCatch(ranges[i], f.Rail, f.Height).Apply(it =>
                        {
                            it.Expression = f.Expression;
                        }));
                }
            }

            //对notes进行排序,按照summonTick从小到大排序
            notes.Sort((a, b) => Math.Sign(b.summonTime - a.summonTime));
            nbtLine.notes = notes;
            //速度
            List<double> speedList = new();
            for (var i = 0; i < track.Count * Settings.currSetting.Tick; i++)
                speedList.Insert(0, line.GetSpeed(i / Settings.currSetting.Tick));
            nbtLine.speedList = speedList;
            lines.Add(nbtLine);
        }

        nbtTrack.lines = lines.ToArray();
        return nbtTrack;
    }

    private static List<Range> AppearTickRange(Note note)
    {
        List<Range> rangeList = new();
        var start = -100000;
        var end = -100000;
        var readyTime = (int)(3 * Settings.currSetting.Tick);
        if (note.type != NoteType.Hold)
        {
            //不是hold
            if (note.Expression != null)
            {
                //自由note
                var expr = note.Expression;
                double position = 0;
                var judgeTick = (int)(note.ActualTime * Settings.currSetting.Tick);
                for (var i = judgeTick; i > -readyTime; i--)
                {
                    expr.Parameters["t"] = judgeTick - i;
                    expr.Parameters["l"] = Settings.currSetting.MapLength;
                    position = Convert.ToDouble(expr.Evaluate() ?? 0);
                    if (end == -100000 && IsInScreen(position)) end = i;
                    if (end != -100000 && start == -100000 && !IsInScreen(position))
                    {
                        start = i;
                        rangeList.Add(new Range(start, end, position));
                        start = end = -100000;
                    }
                }
            }
            else
            {
                //普通note
                double position = 0;
                var judgeTick = (int)(note.ActualTime * Settings.currSetting.Tick);
                for (var i = judgeTick; i > -readyTime; i--)
                {
                    position += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    if (end == -100000 && IsInScreen(position)) end = i;
                    if (end != -100000 && start == -100000 && !IsInScreen(position))
                    {
                        start = i;
                        rangeList.Add(new Range(start, end, position));
                        start = end = -100000;
                    }
                }

                if (end != -100000 && start == -100000)
                    //开头就在屏幕里面了
                    rangeList.Add(new Range(-readyTime, end, position));
            }
        }
        else
        {
            if (note.Expression != null)
            {
                var expr = note.Expression;
                double startPosition = 0, endPosition = 0;
                var judgeTick = (int)(note.ActualTime * Settings.currSetting.Tick);
                var holdTick = (int)(note.ActualHoldTime * Settings.currSetting.Tick);
                //算出当判定结束的时候，判定起始点的位置，同时获取hold长度
                expr.Parameters["t"] = -holdTick;
                expr.Parameters["l"] = Settings.currSetting.MapLength;
                startPosition = Convert.ToDouble(expr.Evaluate() ?? 0);
                var length = -startPosition;
                //迭代开始，划分range
                for (var i = judgeTick + holdTick; i > -readyTime; i--)
                {
                    expr.Parameters["t"] = judgeTick - i;
                    expr.Parameters["l"] = Settings.currSetting.MapLength;
                    endPosition = Convert.ToDouble(expr.Evaluate() ?? 0);
                    startPosition = Convert.ToDouble(expr.Evaluate() ?? 0) + length;
                    //如果起始点或者结束点在屏幕里面，也就是长条离开谱面的时候
                    if (end == -100000 && IsHoldInScreen(startPosition, endPosition)) end = i;
                    //长条进入谱面的时候
                    if (end != -100000 && start == -100000 && !IsHoldInScreen(startPosition, endPosition))
                    {
                        start = i;
                        rangeList.Add(new Range(start, end, startPosition) { length = length });
                        start = end = -100000;
                    }
                }
            }
            else
            {
                double startPosition = 0, endPosition = 0;
                var judgeTick = (int)(note.ActualTime * Settings.currSetting.Tick);
                var holdTick = (int)(note.ActualHoldTime * Settings.currSetting.Tick);
                //算出当判定结束的时候，判定起始点的位置，同时获取hold长度
                for (var i = judgeTick; i < judgeTick + holdTick; i++)
                    startPosition -= note.parentLine.GetSpeed(i / Settings.currSetting.Tick) /
                                     Settings.currSetting.Tick;
                var length = -startPosition;
                //迭代开始，划分range
                for (var i = judgeTick + holdTick; i > -readyTime; i--)
                {
                    endPosition += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    startPosition += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) /
                                     Settings.currSetting.Tick;
                    //如果起始点或者结束点在屏幕里面，也就是长条离开谱面的时候
                    if (end == -100000 && IsHoldInScreen(startPosition, endPosition)) end = i;
                    //长条进入谱面的时候
                    if (end != -100000 && start == -100000 && !IsHoldInScreen(startPosition, endPosition))
                    {
                        start = i;
                        rangeList.Add(new Range(start, end, startPosition) { length = length });
                        start = end = -100000;
                    }
                }

                if (end != -100000 && start == -100000)
                    //开头就在屏幕里面了
                    rangeList.Add(new Range(-readyTime, end, startPosition) { length = length });
            }
        }

        return rangeList;
    }

    private static bool IsInScreen(double position)
    {
        return position > 0 && position < Settings.currSetting.MapLength;
    }

    private static bool IsHoldInScreen(double startPosition, double endPosition)
    {
        return (endPosition > 0 && endPosition < Settings.currSetting.MapLength)
               || (startPosition > 0 && startPosition < Settings.currSetting.MapLength)
               || (startPosition < 0 && endPosition > Settings.currSetting.MapLength);
    }

    public void ToFrameFunctions(DirectoryInfo directory)
    {
        var readyTick = (int)(3 * Settings.currSetting.Tick);
        //生成并初始化函数帧序列
        var frames = new List<string>[(int)(length * Settings.currSetting.Tick) + readyTick + 1];
        for (var i = 0; i < frames.Length; i++) frames[i] = new List<string>();

        //导出帧序列
        foreach (var line in lines)
        foreach (var note in line.notes)
        {
            //计算note的生成时刻
            var startTick = (int)(note.summonTime * Settings.currSetting.Tick);
            if (note is NBTFakeCatch f)
            {
                frames[startTick + readyTick].Add(
                    "summon item_display ~ ~ ~ " +
                    "{" +
                    "UUID:" + Utils.ToNBTUUID(note.uuid) + "," +
                    "transformation:{" +
                    "right_rotation:[1f,0f,0f,0f]," +
                    "scale:[0f,0f,0f]," +
                    "left_rotation:[1f,0f,0f,0f]," +
                    $"translation:[{4 - note.Rail}f, {f.height * 2.0}f, 0f]" +
                    "}," +
                    "item:{" +
                    "id:\"minecraft:leather_boots\"," +
                    "Count:1b," +
                    "components:{custom_model_data:226000}" +
                    "}," +
                    "Tags:[catch,Note,Fake]," +
                    "interpolation_duration:0" +
                    "}");
            }
            else if (note is NBTHold)
            {
                frames[startTick + readyTick].Add(
                    "summon item_display ~ ~ ~ " +
                    "{" +
                    "UUID:" + Utils.ToNBTUUID(note.uuid) + "," +
                    "transformation:{" +
                    "right_rotation:[1f,0f,0f,0f]," +
                    "scale:[0f,0f,0f]," +
                    "left_rotation:[1f,0f,0f,0f]," +
                    "translation:[" + (4 - note.Rail) + "f, 0f, 0f]" +
                    "}," +
                    "item:{" +
                    "id:\"minecraft:leather_leggings\"," +
                    "Count:1b," +
                    "components:{custom_model_data:227001}" +
                    "}," +
                    $"Tags:[hold,Note,{note.Key}]," +
                    "interpolation_duration:0" +
                    "}");
            }
            else if (note.type == (int)NoteType.Catch)
            {
                frames[startTick + readyTick].Add(
                    "summon item_display ~ ~ ~ " +
                    "{" +
                    "UUID:" + Utils.ToNBTUUID(note.uuid) + "," +
                    "transformation:{" +
                    "right_rotation:[1f,0f,0f,0f]," +
                    "scale:[0f,0f,0f]," +
                    "left_rotation:[1f,0f,0f,0f]," +
                    "translation:[" + (4 - note.Rail) + "f, 2f, 0f]" +
                    "}," +
                    "item:{" +
                    "id:\"minecraft:leather_boots\"," +
                    "Count:1b," +
                    "components:{custom_model_data:226001}" +
                    "}," +
                    "Tags:[catch,Note]," +
                    "interpolation_duration:0" +
                    "}");
                frames[startTick + readyTick].Add($"scoreboard players set {note.uuid} PR_slot {note.Rail}");
            }
            else
            {
                frames[startTick + readyTick].Add(
                    "summon item_display ~ ~ ~ " +
                    "{" +
                    "UUID:" + Utils.ToNBTUUID(note.uuid) + "," +
                    "transformation:{" +
                    "right_rotation:[1f,0f,0f,0f]," +
                    "scale:[0f,0f,0f]," +
                    "left_rotation:[1f,0f,0f,0f]," +
                    "translation:[" + (4 - note.Rail) + "f, 0f, 0f]" +
                    "}," +
                    "item:{" +
                    "id:\"minecraft:leather_chestplate\"," +
                    "Count:1b," +
                    "components:{custom_model_data:225001}" +
                    "}," +
                    $"Tags:[tap,Note,{note.Key}]," +
                    "interpolation_duration:0" +
                    "}");
            }

            frames[startTick + readyTick].Add($"ride {note.uuid} mount {NBTLine.uuid}");
            frames[startTick + readyTick]
                .Add(
                    $"scoreboard players set {note.uuid} PR_judgetime {(int)(note.judgeTime * Settings.currSetting.Tick) + readyTick}");
            if (note is NBTHold hold1)
                frames[startTick + readyTick]
                    .Add(
                        $"scoreboard players set {hold1.uuid} PR_holdend {(int)((hold1.judgeTime + hold1.holdTime) * Settings.currSetting.Tick) + readyTick}");
            //计算note所有tick的位置
            var endTick = (int)(note.judgeTime * Settings.currSetting.Tick);
            double distance = 0;
            if (note is NBTHold hold)
            {
                var length = hold.holdLength;
                for (var i = endTick; i > startTick; i--)
                {
                    frames[i + readyTick - 1]
                        .Add($"scoreboard players set {note.uuid} PR_cpos {(int)(distance * 100)}");
                    frames[i + readyTick - 1]
                        .Add($"scoreboard players set {note.uuid} PR_cpos_h {(int)((length + distance) * 100)}");

                    distance += line.line.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                }

                for (var i = endTick + 1; i < endTick + (int)(hold.holdTime * Settings.currSetting.Tick); i++)
                {
                    length -= line.line.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    frames[i + readyTick - 1]
                        .Add($"scoreboard players set {note.uuid} PR_cpos_h {(int)(length * 100)}");
                }
            }
            else
            {
                for (var i = endTick; i > startTick; i--)
                {
                    frames[i + readyTick - 1]
                        .Add($"scoreboard players set {note.uuid} PR_cpos {(int)(distance * 100)}");
                    distance += line.line.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                }

                distance = 0;
                for (var i = endTick + 1; i < endTick + 4 * (Settings.currSetting.Tick / 20) + 1; i++)
                {
                    distance -= line.line.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    frames[i + readyTick - 1]
                        .Add($"scoreboard players set {note.uuid} PR_cpos {(int)(distance * 100)}");
                }
            }
        }

        originalTrack.datapack.Clear();

        for (var i = 0; i < frames.Length; i++)
            originalTrack.datapack.WriteFunction((int)Settings.currSetting.Tick, i, frames[i]);
        //初始化函数
        originalTrack.datapack.WriteInitFunction((int)Settings.currSetting.Tick, new[]
        {
            $"scoreboard players set $time PR_chartinfo {(int)(length * Settings.currSetting.Tick)}",
            $"scoreboard players set $count PR_chartinfo {count}",
            $"summon armor_stand ~ ~ ~ {{UUID:{Utils.ToNBTUUID(NBTLine.uuid)},Tags:[pr_line],NoGravity:1b,Invisible:1b,Marker:1b}}",
            "#判定线的UUID储存",
            $"data modify storage pr:pr_line main.UUID set value {Utils.ToNBTUUID(NBTLine.uuid)}"
        });
    }
}

public class NBTLine
{
    public static Guid uuid = Utils.GeneUnNaturalUUID();
    public Line line;
    public List<NBTNote> notes = new();
    public List<double> speedList = new();
}

public class NBTNote
{
    public List<double> distanceLists = new();

    /// <summary>
    ///     轨迹表达式。如果不存在则为普通的note
    /// </summary>
    public Expression? Expression;

    public byte isFake;

    /// <summary>
    /// note被判定的时间（秒）
    /// </summary>
    public double judgeTime;

    public int Rail;

    /// <summary>
    ///     note出现的时候的位置。主要用于谱面刚刚开始播放的时候部分note已经处于谱面显示范围之中的情况
    /// </summary>
    public double summonPos;

    /// <summary>
    ///     note被生成的时间（秒）
    /// </summary>
    public double summonTime;

    /// <summary>
    ///     note的类型。和NoteType枚举类对应的数字相同
    /// </summary>
    public byte type;

    public Guid uuid = Utils.GeneUnNaturalUUID();

    /// <summary>
    ///     创建一个NBTNote
    /// </summary>
    /// <param name="range">这个Hold从生成到被判定的时间</param>
    /// <param name="isFake">是否是假hold</param>
    /// <param name="type">note的类型</param>
    /// <param name="Rail">所处的轨道</param>
    internal NBTNote(Range range, byte type, int Rail, byte isFake = 0)
    {
        summonTime = range.start / Settings.currSetting.Tick;
        judgeTime = range.end / Settings.currSetting.Tick;
        summonPos = range.startPos;
        this.type = type;
        this.Rail = Rail;
        this.isFake = isFake;
    }

    public string TypeTag => type == (int)NoteType.Tap ? "tap" : "catch";

    public string Key =>
        Rail switch
        {
            7 => "R",
            5 => "F",
            3 => "X",
            1 => "Z",
            _ => "unknown"
        };
}

public class NBTHold : NBTNote
{
    /// <summary>
    ///     hold的长度，不是时间，就是显示出来的长度
    /// </summary>
    public double holdLength;

    public double holdTime;

    /// <summary>
    ///     创建一个NBTHold
    /// </summary>
    /// <param name="range">这个Hold从生成到被判定的时间</param>
    /// <param name="isFake">是否是假hold</param>
    /// <param name="holdTime">按住的时间</param>
    /// <param name="Rail">所处的轨道</param>
    internal NBTHold(Range range, byte isFake, int holdTime, int Rail) : base(range, (int)NoteType.Hold, Rail, isFake)
    {
        this.holdTime = holdTime / Settings.currSetting.Tick;
        holdLength = range.length;
        judgeTime -= this.holdTime;
    }

    public new string TypeTag => "hold";
}

public class NBTFakeCatch : NBTNote
{
    public double height; //假catch高度

    internal NBTFakeCatch(Range range, int Rail, double height)
        : base(range, (int)NoteType.Catch, Rail, 1)
    {
        this.height = height;
    }

    public new string TypeTag => "catch";
}

public class NBTFunction
{
    public string name;
    public double time;

    internal NBTFunction(double time, string name)
    {
        this.time = time;
        this.name = name;
    }
}