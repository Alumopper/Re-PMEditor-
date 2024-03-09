using SharpNBT;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Transactions;

namespace PMEditor.Util
{
    struct Range
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
        public string trackName = "";    //谱面名字
        public string musicAuthor = "";  //曲师
        public string trackAuthor = "";  //谱师
        public double bpm;          //bpm
        public double length;       //曲目长度
        public string difficulty = "";   //谱面难度
        public double count = 0;    //总物量
        public NBTLine[] lines = Array.Empty<NBTLine>();

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
                new IntTag("tickLength",(int)(length * Settings.currSetting.Tick)),
                new DoubleTag("count", count)
            };
            ListTag lineNBT = new("lines", TagType.Compound);
            foreach (var line in lines)
            {
                CompoundTag compound = new("line");
                //notes
                ListTag notesNBT = new("notes", TagType.Compound);
                foreach (NBTNote note in line.notes)
                {
                    CompoundTag noteNBT;
                    if (note is NBTHold hold)
                    {
                        noteNBT = new("hold")
                        {
                            new IntTag("summonTick", (int)(hold.summonTime * Settings.currSetting.Tick)),
                            new IntTag("judgeTick", (int)(hold.judgeTime * Settings.currSetting.Tick)),
                            new DoubleTag("summonPos", hold.summonPos),
                            new ByteTag("isFake", hold.isFake),
                            new DoubleTag("holdTime", hold.holdTime),
                            new DoubleTag("holdLength", hold.holdLength)
                        };
                    }
                    else
                    {
                        noteNBT = new("note")
                        {
                            new IntTag("summonTick", (int)(note.summonTime * Settings.currSetting.Tick)),
                            new IntTag("judgeTick", (int)(note.judgeTime * Settings.currSetting.Tick)),
                            new DoubleTag("summonPos", note.summonPos),
                            new ByteTag("isFake", note.isFake),
                            new ByteTag("type", note.type)
                        };
                    }
                    notesNBT.Add(noteNBT);
                }
                compound.Add(notesNBT);
                lineNBT.Add(compound);
                //速度
                ListTag speedNBT = new("speeds", TagType.Double);
                foreach (double speed in line.speedList)
                {
                    speedNBT.Add(new DoubleTag("speed", speed));
                }
                compound.Add(speedNBT);
            }
            trackNBT.Add(lineNBT);
            return trackNBT;
        }

        public static NBTTrack FromTrack(Track track)
        {
            NBTTrack nbtTrack = new();
            //基本信息
            nbtTrack.trackName = track.TrackName;
            nbtTrack.musicAuthor = track.MusicAuthor;
            nbtTrack.trackAuthor = track.TrackAuthor;
            nbtTrack.bpm = track.Bpm;
            nbtTrack.length = track.Length;
            nbtTrack.difficulty = track.Difficulty;
            nbtTrack.count = track.notesNumber;
            
            //判定线信息
            List<NBTLine> lines = new ();
            foreach (Line line in track.lines)
            {
                NBTLine nbtLine = new()
                {
                    line = line
                };
                //notes
                List<NBTNote> notes = new();
                foreach (Note note in line.notes)
                {
                    var ranges = AppearTickRange(note);
                    if(note.type == NoteType.Hold)
                    {
                        //hold
                        if(ranges.Count == 0)
                        {
                            //一直在屏幕外面
                            //计算hold长度
                            double holdLength = 0;
                            int judgeTick = (int)(note.actualTime * Settings.currSetting.Tick);
                            int holdTick = (int)(note.actualHoldTime * Settings.currSetting.Tick);
                            for (int i = judgeTick; i < judgeTick + holdTick; i++)
                            {
                                holdLength += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                            }
                            notes.Add(
                                item: new NBTHold(
                                    range: new Range(
                                        start: (int)(note.actualTime * Settings.currSetting.Tick), 
                                        end: (int)(note.actualTime * Settings.currSetting.Tick),
                                        startPos: 0
                                        ) { length = holdLength},
                                    isFake: 0, 
                                    holdTick, 
                                    rail: note.rail
                                    )
                                );
                        }
                        else
                        {
                            notes.Add(new NBTHold(ranges[0], 0, (int)(note.actualHoldTime * Settings.currSetting.Tick), note.rail));
                            for (int i = 1; i < ranges.Count; i++)
                            {
                                notes.Add(new NBTHold(ranges[i], 1, (int)(note.actualHoldTime * Settings.currSetting.Tick), note.rail));
                            }   
                        }
                    }
                    else
                    {
                        //不是hold
                        if (ranges.Count == 0)
                        {
                            notes.Add(new NBTNote(new Range((int)(note.actualTime * Settings.currSetting.Tick), (int)(note.actualTime * Settings.currSetting.Tick), 0), (byte)note.NoteType, note.rail, isFake: 0));
                        }
                        else
                        {
                            notes.Add(new NBTNote(ranges[0], (byte)note.NoteType, note.rail, isFake: 0));
                            for (int i = 1; i < ranges.Count; i++)
                            {
                                notes.Add(new NBTNote(ranges[i], (byte)note.NoteType, note.rail, isFake: 1));
                            }
                        }
                    }
                }
                //对notes进行排序,按照summonTick从小到大排序
                notes.Sort((a, b) => Math.Sign(b.summonTime - a.summonTime));
                nbtLine.notes = notes;
                //速度
                List<double> speedList = new();
                for (int i = 0; i < track.length * Settings.currSetting.Tick; i++)
                {
                    speedList.Insert(0, line.GetSpeed(i / Settings.currSetting.Tick));
                }
                nbtLine.speedList = speedList;
                lines.Add(nbtLine);
            }
            nbtTrack.lines = lines.ToArray();
            return nbtTrack;
        }

        private static List<Range> AppearTickRange(Note note)
        {
            List<Range> rangeList = new();
            int start = -1;
            int end = -1;
            if (note.type != NoteType.Hold)
            {
                double position = 0;
                int judgeTick = (int)(note.actualTime * Settings.currSetting.Tick);
                for (int i = judgeTick; i > 0; i--)
                {
                    position += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    if (end == -1 && IsInScreen(position))
                    {
                        end = i;
                    }
                    if (end != -1 && start == -1 && !IsInScreen(position))
                    {
                        start = i;
                        rangeList.Add(new Range(start, end, position));
                        start = end = -1;
                    }
                }
                if (end != -1 && start == -1)
                {
                    //开头就在屏幕里面了
                    rangeList.Add(new Range(0, end, position));
                }
            }
            else
            {
                double startPosition = 0, endPosition = 0;
                int judgeTick = (int)(note.actualTime * Settings.currSetting.Tick);
                int holdTick = (int)(note.actualHoldTime * Settings.currSetting.Tick);
                //算出当判定结束的时候，判定起始点的位置，同时获取hold长度
                for (int i = judgeTick; i < judgeTick + holdTick; i++)
                {
                    startPosition -= note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                }
                double length = -startPosition;
                //迭代开始，划分range
                for (int i = judgeTick + holdTick; i > 0; i--)
                {
                    endPosition += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    startPosition += note.parentLine.GetSpeed(i / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                    //如果起始点或者结束点在屏幕里面
                    if (end == -1 && ((IsInScreen(endPosition) || IsInScreen(startPosition))))
                    {
                        end = i;
                    }
                    if (end != -1 && start == -1 && !IsInScreen(endPosition) && !IsInScreen(startPosition))
                    {
                        start = i;
                        rangeList.Add(new Range(start, end, startPosition) { length = length});
                        start = end = -1;
                    }
                }
                if (end != -1 && start == -1)
                {
                    //开头就在屏幕里面了
                    rangeList.Add(new Range(0, end, startPosition) { length = length});
                }
            }
            return rangeList;
        }

        private static bool IsInScreen(double position)
        {
            return position > 0 && position < Settings.currSetting.MapLength;
        }

        public void ToFrameFunctions(DirectoryInfo directory)
        {
            //生成并初始化函数帧序列
            List<string>[] frames = new List<string>[(int)(length * Settings.currSetting.Tick)];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new();
            }

            //导出帧序列
            foreach(NBTLine line in lines)
            {
                foreach(NBTNote note in line.notes)
                {
                    //计算note的生成时刻
                    int startTick = (int)(note.summonTime * Settings.currSetting.Tick);
                    frames[startTick].Add("summon item_display ~ ~ ~ {UUID:" + Utils.ToNBTUUID(note.uuid) + ",transformation:{scale:[0.0f,0.0f,0.0f]},item:{id:\"minecraft:leather_chestplate\",Count:1b,tag:{CustomModelData:225001}},item_display:\"head\",Tags:[\"tap\",\"PR_just\",\"Note\",\"R\"],interpolation_duration:1}");
                    frames[startTick].Add($"ride {note.uuid} mount {line.uuid}");
                    //计算note所有tick的位置
                    int endTick = (int)(note.judgeTime * Settings.currSetting.Tick);
                    double distance = 0;
                    for(; endTick >= startTick; endTick--)
                    {
                        distance += line.line.GetSpeed(endTick / Settings.currSetting.Tick) / Settings.currSetting.Tick;
                        frames[endTick].Add($"data modify entity {note.uuid} tranformation.translation[0] set value {(distance > 0 ? distance : 0)}");
                        frames[endTick].Add($"scoreboard players set {note.uuid} PR_cpos {(int)(distance * 1000)}");
                        if (note is NBTHold hold)
                        {
                            //尾部位置
                            frames[endTick].Add($"scoreboard players set {note.uuid} PR_cpos_h {((int)(distance + hold.holdLength))}");
                        }
                    }
                }
            }

            DirectoryInfo output = new(directory.FullName + "/" + trackName);
            //输出全部的序列
            DirectoryInfo frameDir = new(output.FullName + "/frames");
            frameDir.Create();
            for (int i = 0; i < frames.Length; i++)
            {
                File.WriteAllLines(frameDir.FullName + $"/{i}.mcfunction", frames[i]);
            }
            //初始化函数
            File.WriteAllLines(output.FullName + "/init.mcfunction", new string[] {
                $"scoreboard players set $time PR_chartinfo {(int)(length * Settings.currSetting.Tick)}" ,
                $"scoreboard players set $count PR_chartinfo {count}"
            });
        }
    }

    public class NBTLine
    {
        public Line line;
        public List<NBTNote> notes = new();
        public List<double> speedList = new();
        public Guid uuid = Guid.NewGuid(); 
    }

    public class NBTNote
    {
        /// <summary>
        /// note的类型。和NoteType枚举类对应的数字相同
        /// </summary>
        public byte type;
        /// <summary>
        /// note被生成的时间（秒）
        /// </summary>
        public double summonTime;
        /// <summary>
        /// note被判定的时间（秒）
        /// </summary>
        public double judgeTime;
        public byte isFake;
        /// <summary>
        /// note出现的时候的位置。主要用于谱面刚刚开始播放的时候部分note已经处于谱面显示范围之中的情况
        /// </summary>
        public double summonPos;
        public int rail;

        public List<double> distanceLists = new();

        public Guid uuid = Guid.NewGuid();

        internal NBTNote(Range range, byte type, int rail, byte isFake = 0)
        {
            summonTime = range.start / Settings.currSetting.Tick;
            judgeTime = range.end / Settings.currSetting.Tick;
            this.summonPos = range.startPos;
            this.type = type;
            this.rail = rail;
            this.isFake = isFake;
        }
    }

    public class NBTHold : NBTNote
    {
        public double holdTime;
        public double holdLength;   //hold的长度，不是时间，就是显示出来的长度

        internal NBTHold(Range range, byte isFake, int holdTime, int rail) : base(range, (int)NoteType.Hold, rail, isFake)
        {
            this.holdTime = holdTime/Settings.currSetting.Tick;
            this.holdLength = range.length;
            this.judgeTime -= this.holdTime;
        }
    }
}
