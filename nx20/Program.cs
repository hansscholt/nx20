using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nx20
{
    class Program
    {
        string[] sItem = new string[] { "Heart", "Potion", "Mine", "Velocity", "Extra" ,"Direction"};
        int[] iItem = new int[6];

        bool bSkin;// no configurable

        //opciones
        static int iDivision = -1;
        //-1 son bloque random, puedes elegir el índice
        static bool bAllStepBulk = false;
        static string sBank1 = "perfor1";
        static string sBank2 = "perfor2";
        static int iCommonBeatSplit = 8;


        static bool bFakeNotesDivision;
        static bool bFakeNotesFlag;
        static int iLastDivision;


        string[] sDiff;
        static void Main(string[] args)
        {
            Console.WriteLine("--NX20 converter by HANS--");
            bFakeNotesFlag = false;
            IniFile iniFile = new IniFile("config.ini");
            if (!iniFile.KeyExists("DivisionBlock", "Config"))
            {
                iDivision = -1;
                iniFile.Write("DivisionBlock", "-1", "Config");
            }
            else
                iDivision = int.Parse(iniFile.Read("DivisionBlock", "Config"));
            if (!iniFile.KeyExists("AllStepBulk", "Config"))
            {
                bAllStepBulk = false;
                iniFile.Write("AllStepBulk", "false", "Config");
            }
            else
                bAllStepBulk = bool.Parse(iniFile.Read("AllStepBulk", "Config"));
            if (!iniFile.KeyExists("SkinBank1", "Config"))
            {
                sBank1 = "perfor1";
                iniFile.Write("SkinBank1", "perfor1", "Config");
            }
            else
                sBank1 = iniFile.Read("SkinBank1", "Config");
            if (!iniFile.KeyExists("SkinBank2", "Config"))
            {
                sBank2 = "perfor2";
                iniFile.Write("SkinBank2", "perfor2", "Config");
            }
            else
                sBank2 = iniFile.Read("SkinBank2", "Config");
            if (!iniFile.KeyExists("CommonBeatSplit", "Config"))
            {
                iCommonBeatSplit = 8;
                iniFile.Write("CommonBeatSplit", "8", "Config");
            }
            else
                iCommonBeatSplit = int.Parse(iniFile.Read("CommonBeatSplit", "Config"));
            Program p = new Program();
            string sDirName = new DirectoryInfo(Directory.GetCurrentDirectory()).Name;



            //byte[] decompressed = p.vvv();
            //StringBuilder hex = new StringBuilder(decompressed.Length * 2);
            //foreach (byte b in decompressed)
            //    hex.AppendFormat("{0:x2}", b);
            //string abc = hex.ToString();
            //File.WriteAllBytes("asd.gz", decompressed);


            p.sDiff = new string[] { "Beginner", "Easy", "Medium", "Hard", "Challenge", "Extra1", "Extra2" , "Extra3", "Extra4" };

            int iFiles = 0;
            if (args.Length > 0)//arrastré el archivo
            {
                using (StreamWriter sw = new StreamWriter("STEP.SSC", false))
                {
                    p.SongInfo(sw, sDirName);
                }
                foreach (string sPath  in args)
                {
                    string s = Path.GetFileName(sPath);
                    if (s.ToUpper() == "LM.NX")
                        continue;
                    Console.Write(">>>>" + s);
                    if (bAllStepBulk)
                    {
                        int iL = 1;
                        foreach (string sDiff in p.sDiff)
                        {
                            if (p.Read(s, sDiff, iL))
                            {
                                iFiles++;
                                Console.WriteLine();
                            }
                            else
                                Console.WriteLine("\t error");
                            iL++;
                        }

                    }
                    else
                    {
                        if (p.Read(s))
                        {
                            iFiles++;
                            Console.WriteLine();
                        }
                        else
                            Console.WriteLine("\t error");
                    }
                }               
            }
            else
            {
                //string[] sFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.stx");
                string[] sFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.NX");
                if (sFiles.Length > 0)
                {
                    using (StreamWriter sw = new StreamWriter("STEP.SSC", false))
                    {
                        p.SongInfo(sw, sDirName);
                    }
                    foreach (var sPath in sFiles)
                    {
                        string s = Path.GetFileName(sPath);
                        if (s.ToUpper() == "LM.NX")
                            continue;
                        Console.Write(">>>>" + s);
                        if (bAllStepBulk)
                        {
                            int iL = 1;
                            foreach (string sDiff in p.sDiff)
                            {
                                if (p.Read(s, sDiff, iL))
                                {
                                    iFiles++;
                                    Console.WriteLine();
                                }
                                else
                                    Console.WriteLine("\t error");
                                iL++;
                            }
                            
                        }
                        else
                        {
                            if (p.Read(s))
                            {
                                iFiles++;
                                Console.WriteLine();
                            }
                            else
                                Console.WriteLine("\t error");
                        }
                    }
                }
            }

            if (iFiles == 0)
            {
                Console.WriteLine("no files found");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("ready");
                Console.ReadKey();
            }
        }

        byte[] vvv()
        {
            byte[] file = File.ReadAllBytes("noname");





            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(file),
                CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }

        }

        void SongInfo(StreamWriter sw, string sDirName)
        {
            sw.WriteLine("#VERSION:0.83;");
            sw.WriteLine("#TITLE:" + sDirName + ";");
            sw.WriteLine("#SUBTITLE:;");
            sw.WriteLine("#ARTIST:ARTIST;");
            sw.WriteLine("#TITLETRANSLIT:;");
            sw.WriteLine("#SUBTITLETRANSLIT:;");
            sw.WriteLine("#ARTISTTRANSLIT:;");
            sw.WriteLine("#CUSTOMBPM:000;");
            sw.WriteLine("#HEARTS:2;");
            sw.WriteLine("#CATEGORY:NEWTUNES;");
            sw.WriteLine("#SPECIAL:;");
            sw.WriteLine("#GENRE:;");
            sw.WriteLine("#ORIGIN:;");
            sw.WriteLine("#CREDIT:;");
            //sw.WriteLine("#BANNER:;");
            sw.WriteLine("#BANNER:BANNER.JPG;");
            //sw.WriteLine("#BACKGROUND:TITLE.PNG;");
            sw.WriteLine("#BACKGROUND:T" + sDirName + ".PNG;");
            sw.WriteLine("#PREVIEWVID:;");
            sw.WriteLine("#JACKET:;");
            sw.WriteLine("#CDIMAGE:;");
            sw.WriteLine("#DISCIMAGE:;");
            sw.WriteLine("#LYRICSPATH:;");
            sw.WriteLine("#CDTITLE:;");
            //sw.WriteLine("#MUSIC:SONG.MP3;");
            sw.WriteLine("#MUSIC:" + sDirName + ".MP3;");
            //sw.WriteLine("#PREVIEW:DEMO.MP3;");
            sw.WriteLine("#PREVIEW:D" + sDirName + ".MP3;");
            sw.WriteLine("#OFFSET:0.00000;");
            sw.WriteLine("#SAMPLESTART:0.000000;");
            sw.WriteLine("#SAMPLELENGTH:12.000000;");
            sw.WriteLine("#SELECTABLE:YES;");
            sw.WriteLine("#BPMS:0.000=60.000;");
            sw.WriteLine("#STOPS:;");
            sw.WriteLine("#DELAYS:;");
            sw.WriteLine("#WARPS:;");
            sw.WriteLine("#TIMESIGNATURES:0.000=4=4;");
            sw.WriteLine("#TICKCOUNTS:0.000=4;");
            sw.WriteLine("#COMBOS:0.000=1;");
            sw.WriteLine("#SPEEDS:0.000=1.000=0.000=0;");
            sw.WriteLine("#SCROLLS:0.000=1.000;");
            sw.WriteLine("#FAKES:;");
            sw.WriteLine("#LABELS:0.000=Song Start;");
            sw.WriteLine("#BGCHANGES:0.000=" + sDirName + ".MPG=1.000=0=0=0=StretchNoLoop====,");
            sw.WriteLine("99999 = -nosongbg -= 1.000 = 0 = 0 = 0 // don't automatically add -songbackground-;");
            //sw.WriteLine("#BGCHANGES:;");
            sw.WriteLine("#FGCHANGES:;");
            sw.WriteLine("#KEYSOUNDS:;");
            sw.WriteLine("#ATTACKS:;");
            sw.WriteLine();
        }

        char CommonLayer(int i)
        {
            switch (i)
            {
                case 0: //tap hidden
                    return '3';
                case 1: //tap appear
                    return '1';
                case 2: //tap vanish
                    return '2';
                case 3: //tap
                    return '0';
                case 19: //zigzag
                    return 'y';
                default:
                    return '!';
            }
        }

        char BonusLayer(int i)
        {
            switch (i)
            {
                case 0: //bonus hidden
                    return '7';
                case 1: //bonus appear
                    return '5';
                case 2: //bonus vanish
                    return '6';
                case 3: //bonus
                    return '4';
                default:
                    return '!';
            }
        }

        char GhostLayer(int i)
        {
            switch (i)
            {
                //case 0: //ghost hidden
                //    return '!';
                case 1: //ghost appear
                    return '9';
                case 2: //ghost vanish
                    return 'a';
                case 3: //ghost
                    return '8';
                default:
                    return '!';
            }
        }

        char CommonSpecial(int i)
        {
            switch (i)
            {
                case 0: //G
                    return 'G';
                case 1: //W
                    return 'W';
                case 2: //A
                    return 'z';//action
                case 3: //B
                    return 'x';//shield
                case 4: //C
                    return 'c';//change
                default:
                    return '!';
            }
        }
        char CommonSpecialNX10(int i)
        {
            switch (i)
            {
                case 3: //G
                    return 'G';
                case 7: //W
                    return 'W';
                case 11: //A
                    return 'z';//action
                case 15: //B
                    return 'x';//shield
                case 19: //C
                    return 'c';//change
                default:
                    return '!';
            }
        }

        char CommonItemNX10(int i)
        {
            //iDELETE++;
            switch (i)
            {
                case 3: iItem[4]++; return 'z'; //action
                case 7: iItem[4]++; return 'x'; //shield
                case 11: iItem[4]++; return 'c'; //change
                case 15: iItem[4]++; return 'y'; //squid
                case 19: iItem[4]++; return 'k'; //weather
                case 23: iItem[2]++; return 'M'; //mine
                case 27: return '!'; //mine layer
                case 39: iItem[0]++; return 'h'; //heart
                case 43: iItem[3]++; return 's'; //x2
                case 47: iItem[4]++; return 'b'; //random
                case 51: iItem[3]++; return 'd'; //x3
                case 55: iItem[3]++; return 'f'; //x4
                case 59: iItem[3]++; return 'g'; //x8
                case 63: iItem[3]++; return 'a'; //x1
                case 67: iItem[1]++; return 'p'; //potion
                case 71: iItem[5]++; return 'e'; //up
                case 75: iItem[5]++; return 'r'; //right
                case 79: iItem[5]++; return 'w'; //down
                case 83: iItem[5]++; return 'q'; //left
                case 31: iItem[4]++; return 'l'; //rocket
                case 35: iItem[4]++; return 'm'; //drain
                default: return '!';
            }
            
        }

        char CommonItem(int i)
        {
            switch (i)
            {
                case 0: iItem[4]++; return 'z'; //action
                case 1: iItem[4]++; return 'x'; //shield
                case 2: iItem[4]++; return 'c'; //change
                case 3: iItem[4]++; return 'y'; //acceleration (squid)
                case 4: iItem[4]++; return 'k'; //weather
                case 5: iItem[2]++; return 'M'; //mine
                case 6: iItem[2]++; return 'M'; //mine field???
                case 7: iItem[2]++; return 'l'; //rocket
                case 8: iItem[2]++; return 'm'; //drain
                case 9: iItem[0]++; return 'h'; //heart
                case 10: iItem[3]++; return 's'; //x2
                case 11: iItem[4]++; return 'b'; //random
                case 12: iItem[3]++; return 'd'; //x3
                case 13: iItem[3]++; return 'f'; //x4
                case 14: iItem[3]++; return 'g'; //x8
                case 15: iItem[3]++; return 'a'; //x1
                case 16: iItem[1]++; return 'p'; //potion
                case 17: iItem[5]++; return 'e'; //up
                case 18: iItem[5]++; return 'r'; //right
                case 19: iItem[5]++; return 'w'; //down
                case 20: iItem[5]++; return 'q'; //left
                default: return '!';
            }
        }

        char CommonBank(int i)
        {
            switch (i)
            {
                case 0: //bank 0
                    return '0';
                case 64: //bank 1
                case 1: //bank 1
                    bSkin = true;
                    return BankSkin(sBank1);
                case 128: //bank 2
                case 2: //bank 2
                    bSkin = true;
                    return BankSkin(sBank2);
                default:
                    return '!';
            }
        }

        char BankSkin(string sBank)
        {
            switch (sBank)
            {
                case "perfor3": return '3';
                case "soccer": return '4';
                case "drumblue": return '6';
                case "drumred": return '6';
                case "drumyellow": return '7';
                case "flower": return '8';
                case "old": return '9';
                case "easy": return 'a';
                case "slime": return 'b';
                case "canon": return 'c';
                case "poker": return 'd';
                case "music": return 'e';
                case "nx": return 'f';
                case "sheep": return 'g';
                case "horse": return 'h';
                case "dog": return 'i';
                case "girl": return 'j';
                case "wind": return 'm';
                case "nxa": return 'n';
                case "nx2": return 'o';
                case "lightning": return 'p';
                case "drum": return 'q';
                case "missile": return 'r';
                case "rebirth": return 's';
                case "basic": return 't';
                case "fiesta": return 'u';
                case "fiesta2": return 'v';
                case "prime2": return 'w';
                case "fire": return 'k';
                case "ice": return 'l';
                case "perfor1": return '1';
                case "perfor2": return '2';
            }
            return '0';
        }

        string NoteNX10(Step s, int split)
        {
            if (s.iNote == 0)
                return "0";

            bool bSpecialLayer = false;

            char[] c = new char[] { '0', '0', '0' };

            if (s.iNote == 179)// TAP
            {
                c[0] = '1';
            }
            else if (s.iNote == 147)// APPEAR TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '1';
            }
            else if (s.iNote == 163)// VANISH TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '2';
            }
            else if (s.iNote == 131)// HIDDEN TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '3';
            }
            else if (s.iNote == 243)// BONUS TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '4';
            }
            else if (s.iNote == 211)// BONUS APPEAR TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '5';
            }
            else if (s.iNote == 227)// BONUS VANISH TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '6';
            }
            else if (s.iNote == 195)// BONUS HIDDEN TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '7';
            }
            else if (s.iNote == 115)// GHOST TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '8';
            }
            else if (s.iNote == 83)// GHOST APPEAR TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = '9';
            }
            else if (s.iNote == 99)// GHOST VANISH TAP
            {
                bSpecialLayer = true;
                c[0] = '1';
                c[2] = 'a';
            }
            ///////////////////////////////////////////HOLD HEAD////////////////////////////////////////////
            else if (s.iNote == 180)// HOLD HEAD
            {
                c[0] = '2';
            }
            else if (s.iNote == 148)// HOLD HEAD APPEAR
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '1';
            }
            else if (s.iNote == 164)// HOLD HEAD VANISH
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '2';
            }
            else if (s.iNote == 132)// HOLD HEAD HIDDEN
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '3';
            }
            else if (s.iNote == 244)// HOLD HEAD BONUS
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '4';
            }
            else if (s.iNote == 212)// HOLD HEAD BONUS APPEAR
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '5';
            }
            else if (s.iNote == 228)// HOLD HEAD BONUS VANISH
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '6';
            }
            else if (s.iNote == 196)// HOLD HEAD BONUS HIDDEN
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '7';
            }
            else if (s.iNote == 116)// HOLD HEAD GHOST
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '8';
            }
            else if (s.iNote == 84)// HOLD HEAD GHOST APPEAR
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = '9';
            }
            else if (s.iNote == 100)// HOLD HEAD GHOST VANISH
            {
                bSpecialLayer = true;
                c[0] = '2';
                c[2] = 'a';
            }
            ///////////////////////////////////////////HOLD BODY////////////////////////////////////////////
            else if (s.iNote == 182)// HOLD BODY
            {
                c[0] = '0';
            }
            else if (s.iNote == 150)// HOLD BODY APPEAR
            {
                c[0] = '0';
            }
            else if (s.iNote == 166)// HOLD BODY VANISH
            {
                c[0] = '0';
            }
            else if (s.iNote == 134)// HOLD BODY HIDDEN
            {
                c[0] = '0';
            }
            else if (s.iNote == 246)// HOLD BODY BONUS
            {
                c[0] = '0';
            }
            else if (s.iNote == 214)// HOLD BODY BONUS APPEAR
            {
                c[0] = '0';
            }
            else if (s.iNote == 230)// HOLD BODY BONUS VANISH
            {
                c[0] = '0';
            }
            else if (s.iNote == 198)// HOLD BODY BONUS HIDDEN
            {
                c[0] = '0';
            }
            else if (s.iNote == 118)// HOLD BODY GHOST
            {
                c[0] = '0';
            }
            else if (s.iNote == 86)// HOLD BODY GHOST APPEAR
            {
                c[0] = '0';
            }
            else if (s.iNote == 102)// HOLD BODY GHOST VANISH
            {
                c[0] = '0';
            }
            ///////////////////////////////////////////HOLD TAIL////////////////////////////////////////////
            else if (s.iNote == 183)// HOLD TAIL
            {
                c[0] = '3';
            }
            else if (s.iNote == 151)// HOLD TAIL APPEAR
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '1';
            }
            else if (s.iNote == 167)// HOLD TAIL VANISH
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '2';
            }
            else if (s.iNote == 135)// HOLD TAIL HIDDEN
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '3';
            }
            else if (s.iNote == 247)// HOLD TAIL BONUS
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '4';
            }
            else if (s.iNote == 215)// HOLD TAIL BONUS APPEAR
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '5';
            }
            else if (s.iNote == 231)// HOLD TAIL BONUS VANISH
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '6';
            }
            else if (s.iNote == 199)// HOLD TAIL BONUS HIDDEN
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '7';
            }
            else if (s.iNote == 119)// HOLD TAIL GHOST
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '8';
            }
            else if (s.iNote == 87)// HOLD TAIL GHOST APPEAR
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = '9';
            }
            else if (s.iNote == 103)// HOLD TAIL GHOST VANISH
            {
                bSpecialLayer = true;
                c[0] = '3';
                c[2] = 'a';
            }
            ///////////////////////////////////////////SPECIAL LETTER////////////////////////////////////////////
            else if (s.iNote == 242)// BONUS LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = '4';
            }
            else if (s.iNote == 210)// BONUS APPEAR LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = '5';
            }
            else if (s.iNote == 226)// BONUS VANISH LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = '6';
            }
            else if (s.iNote == 194 || s.iNote == 178)// BONUS HIDDEN LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = '7';
            }
            else if (s.iNote == 114)// GHOST LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = '8';
            }
            else if (s.iNote == 82)// GHOST APPEAR LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = '9';
            }
            else if (s.iNote == 98)// GHOST VANISH LETTER
            {
                bSpecialLayer = true;
                c[0] = CommonSpecialNX10(s.iLayer);
                c[2] = 'a';
            }
            else if (s.iNote == 241)// BONUS ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = '4';
            }
            else if (s.iNote == 209)// BONUS APPEAR ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = '5';
            }
            else if (s.iNote == 225)// BONUS VANISH ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = '6';
            }
            else if (s.iNote == 193)// BONUS HIDDEN ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = '7';
            }
            else if (s.iNote == 113)// GHOST ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = '8';
            }
            else if (s.iNote == 81)// GHOST APPEAR ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = '9';
            }
            else if (s.iNote == 97)// GHOST VANISH ITEM
            {
                bSpecialLayer = true;
                c[0] = CommonItemNX10(s.iLayer);
                c[2] = 'a';
            }
            else
            {
                Console.WriteLine("CANT PARSE NOTE>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
            }

            if (s.iLayer == 5)// BANK1
            {
                bSpecialLayer = true;
                c[1] = BankSkin(sBank1);
                bSkin = true;
            }
            else if (s.iLayer == 10)// BANK2
            {
                bSpecialLayer = true;
                c[1] = BankSkin(sBank2);
                bSkin = true;
            }
            //else
            //{

            //}



            if (bSpecialLayer)
                return "{" + new string(c) + "}";
            else
                return c[0].ToString();
        }

        string NoteSTF4(Step s, int split)
        {
            if (s.iNote == 0)
                return "0";
            if (s.iNote == 1)//tap
                return "1";
            if (s.iNote == 10)//holdhead
                return "2";
            if (s.iNote == 11)//holdbody
                return "0";
            if (s.iNote == 12)//holdtail
                return "3";
            return "";
        }

        string NoteNX20(Step s, int split)
        {
            if (s.iNote == 0)
                return "0";


            //bueno no entiendo como andamiro maneja su coso, al parecer es al lote

            bool bSpecialLayer = false;

            char[] c = new char[] { '0', '0', '0' };

            if (s.iSpecial == 192)//será flag? (ITEM Y SPECIAL)
            {
                if (s.iNote == 97)//item bonus
                {
                    bSpecialLayer = true;
                    c[0] = CommonItem(s.iPlayer);
                    if (c[0] == '!')
                    {
                        Console.WriteLine("CANT PARSE 97i>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = BonusLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 97l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 33)//item ghost
                {
                    bSpecialLayer = true;

                    c[0] = CommonItem(s.iPlayer);
                    if (c[0] == '!')
                    {
                        Console.WriteLine("CANT PARSE 33i>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                    c[2] = GhostLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 33l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 65)//item normal
                {
                    bSpecialLayer = true;
                    c[0] = CommonItem(s.iPlayer);
                    if (c[0] == '!')
                    {
                        Console.WriteLine("CANT PARSE 65i>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = BonusLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 65l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 66)//special(letras) //normal TAP
                {
                    c[0] = CommonSpecial(s.iPlayer);
                    if (c[0] == '!')
                    {
                        Console.WriteLine("CANT PARSE 66s>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = CommonLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 66l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 98)//special(letras) //bonus TAP
                {
                    bSpecialLayer = true;
                    c[0] = CommonSpecial(s.iPlayer);
                    if (c[0] == '!')
                    {
                        Console.WriteLine("CANT PARSE 98s>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = BonusLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 98l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else
                {
                    Console.WriteLine("CANT PARSE 192>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                }
            }
            else
            {
                if (s.iNote == 67)//tap
                {
                    //bSpecialLayer = true;
                    c[0] = '1';
                    c[2] = CommonLayer(s.iLayer);
                    if (c[2] != '0')
                        bSpecialLayer = true;

                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 67l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    //c[1] = CommonBank(s.iSpecial);
                    c[1] = CommonBank(s.iPlayer);
                    if (c[1] != '0')
                        bSpecialLayer = true;
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 67b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                }
                else if (s.iNote == 87)//hold head
                {
                    c[0] = '2';
                    c[2] = CommonLayer(s.iLayer);
                    if (c[2] != '0')
                        bSpecialLayer = true;

                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 87l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    //c[1] = CommonBank(s.iSpecial);
                    c[1] = CommonBank(s.iPlayer);
                    if (c[1] != '0')
                        bSpecialLayer = true;
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 87b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 91)//hold body
                {
                    c[0] = '0';
                }
                else if (s.iNote == 95)//hold tail
                {
                    //bSpecialLayer = true;
                    c[0] = '3';
                    c[2] = CommonLayer(s.iLayer);
                    if (c[2] != '0')
                        bSpecialLayer = true;

                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 95l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    //c[1] = CommonBank(s.iSpecial);
                    c[1] = CommonBank(s.iPlayer);
                    if (c[1] != '0')
                        bSpecialLayer = true;
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 95b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 35)//tap ghost
                {
                    bSpecialLayer = true;
                    c[0] = '1';

                    c[1] = CommonBank(s.iPlayer);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 35b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = GhostLayer(s.iLayer);
                    if (c[2] == '!') //tap ghost vanish
                    {
                        Console.WriteLine("CANT PARSE 35l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 55)//head ghost
                {
                    bSpecialLayer = true;
                    c[0] = '2';

                    c[1] = CommonBank(s.iPlayer);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 55b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = GhostLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 55l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 59)//body ghost
                {
                    ;
                    c[0] = '0';
                }
                else if (s.iNote == 63)//tail ghost
                {
                    bSpecialLayer = true;
                    c[0] = '3';

                    c[1] = CommonBank(s.iPlayer);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 63b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = GhostLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 63l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 99)//tap bonus 
                {
                    bSpecialLayer = true;
                    c[0] = '1';

                    c[1] = CommonBank(s.iSpecial);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 99b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = BonusLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 99l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 119)//head bonus
                {
                    bSpecialLayer = true;
                    c[0] = '2';

                    c[1] = CommonBank(s.iSpecial);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 119b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = BonusLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 119l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 123)//body bonus
                {
                    //bSpecialLayer = true;
                    c[0] = '0';
                    //c[2] = '7';
                }
                else if (s.iNote == 127)//tail bonus
                {
                    bSpecialLayer = true;
                    c[0] = '3';

                    c[1] = CommonBank(s.iSpecial);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 127b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }

                    c[2] = BonusLayer(s.iLayer);
                    if (c[2] == '!')
                    {
                        Console.WriteLine("CANT PARSE 127l>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                }
                else if (s.iNote == 71)//head roll
                    //excepción acá todos los roll los voy a hacer bonus
                {
                    bSpecialLayer = true;
                    c[0] = '4';

                    c[1] = CommonBank(s.iSpecial);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 71b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                    c[2] = '4';
                }
                else if (s.iNote == 75)//roll body
                                       //excepción acá todos los roll los voy a hacer bonus
                {
                    c[0] = '0';
                }
                else if (s.iNote == 79)//tail roll
                                       //excepción acá todos los roll los voy a hacer bonus
                {
                    bSpecialLayer = true;
                    c[0] = '3';

                    c[1] = CommonBank(s.iSpecial);
                    if (c[1] == '!')
                    {
                        Console.WriteLine("CANT PARSE 79b>>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                            + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                    }
                    c[2] = '4';
                }
                else
                {
                    Console.WriteLine("CANT PARSE >>>> Note:" + s.iNote + "\tLayer:" + s.iLayer + "\tPlayer:" + s.iPlayer + "\tSpecial:" + s.iSpecial
                        + "\tSplit:" + (split + 1) + "\tRow:" + (s.iRow + 1) + "\tCol:" + (s.iCol + 1));
                }
            }

            if (bSpecialLayer)            
                return "{" + new string(c) + "}";
            else
                return c[0].ToString();
        }

        bool WriteStep(List<SplitData> splitData, int iCols, int iType, string sFile, int iLevel, string sDiff, int iL)
        {
            if (splitData.Count == 0)
                return false;

            bSkin = false;

            using (StreamWriter sw = new StreamWriter("STEP.SSC", true))
            {
                string sDifficulty = "Edit";

                if (iCols < 5)
                    return false;

                if (iType == 0)
                    Console.WriteLine("\t NX10...");
                if (iType == 1)
                    Console.WriteLine("\t NX20...");
                if (iType == 2)
                    Console.WriteLine("\t STF4..." + sFile.Replace(".NX", ""));

                string sStepType = "pump-single";
                if (iCols == 6)
                    sStepType = "pump-halfdouble";
                if (iCols == 10)
                    sStepType = "pump-double";

                if (sFile.ToUpper() == "NO.NX")
                {
                    sDifficulty = "Beginner";
                    iLevel = 1;
                }
                else if (sFile.ToUpper() == "HD.NX")
                {
                    sDifficulty = "Easy";
                    iLevel = 2;
                }
                else if (sFile.ToUpper() == "CR.NX")
                {
                    sDifficulty = "Medium";
                    iLevel = 3;
                }
                else if (sFile.ToUpper() == "FR.NX")
                {
                    sDifficulty = "Beginner";
                    iLevel = 1;
                }
                else if (sFile.ToUpper() == "NM.NX")
                {
                    sDifficulty = "Easy";
                    iLevel = 2;
                }
                else if (sFile.ToUpper() == "HF.NX")
                {
                    sDifficulty = "Beginner";
                    iLevel = 1;
                }

                string sChartName = "";
                if (sDiff != null)
                {
                    sDifficulty = sDiff;
                    iLevel = iL;
                    sChartName = "STEP" + iL;
                }


                if (iType == 2)
                    sFile = sFile.Replace(".NX", "");

                sw.WriteLine("//---------------pump-single - " + sFile + "----------------");
                sw.WriteLine("#NOTEDATA:;");
                sw.WriteLine("#CHARTNAME:" + sChartName + ";");
                sw.WriteLine("#STEPSTYPE:" + sStepType + ";");
                sw.WriteLine("#DESCRIPTION:" + sFile + ";");
                sw.WriteLine("#LABELTYPE:NORMAL;");
                //sw.WriteLine("#PRELOADNOTESKIN:;");
                sw.WriteLine("#SEPARATENOTES:NO;");
                sw.WriteLine("#PLAYERS:1;");
                sw.WriteLine("#QUESTDESC:;");
                sw.WriteLine("#QUESTMODS:;");
                sw.WriteLine("#QUESTBARINITIAL:0.500;");
                sw.WriteLine("#QUESTBARLEVEL:0;");
                sw.WriteLine("#CHARTSTYLE:;");
                sw.WriteLine("#DIFFICULTY:" + sDifficulty + ";");
                sw.WriteLine("#METER:" + iLevel.ToString() + ";");
                sw.WriteLine("#RADARVALUES:;");
                sw.WriteLine("#CREDIT:HANS NX20;");
                sw.WriteLine("#OFFSET:" + (splitData[0].divisionData[0].timming.fOffset / -1000.0f).ToString() + ";");

                List<float> fOFFSET = new List<float>();
                List<float> fBPM = new List<float>();
                List<float> fBEAT = new List<float>();
                List<float> fDELAY = new List<float>();
                List<float> fFREEZE = new List<float>();
                List<int> iBEATSPLIT = new List<int>();//no se usa
                List<float> fSPEED = new List<float>();
                List<float> fSCROLL = new List<float>();
                List<float> fMYSTERY = new List<float>();
                List<int> iSMOOTH = new List<int>();
                List<int> iROWS = new List<int>();
                List<float> fWARP = new List<float>();

                string sCol = null;
                for (int c = 0; c < iCols; c++)
                    sCol += "0";

                float fCurrentBeat = 0;

                StringBuilder sNOTES = new StringBuilder();

                int iRow = 0;
                Random r;
                for (int sd = 0; sd < splitData.Count; sd++)
                {

                    //para el timming data sacar el primer division, andamiro solamente le pone timming data al primer step(NI IDEA POR QUE)
                    //para las notas sacar random division data
                    r = new Random(Guid.NewGuid().GetHashCode());
                    int iD = iDivision;
                    if (iDivision < -1 || iDivision == 999)
                        iDivision = -1;



                    if (!bFakeNotesDivision)
                    {
                        if (iDivision == -1)
                            iD = r.Next(0, splitData[sd].divisionData.Count);
                    }
                    else
                    {
                        iD = iLastDivision;
                    }

                    if (iD < 0)
                        iD = 0;
                    if (iD >= splitData[sd].divisionData.Count)
                        iD = splitData[sd].divisionData.Count - 1;

                    //if (iDivision == 999)
                    //{
                    //    iD = iAuxDivision;
                    //    //iAuxDivision++;
                    //}



                    DivisionData divisionData = splitData[sd].divisionData[iD];
                    float fDif = (float)divisionData.timming.iBeatSplit / (float)iCommonBeatSplit;

                    fBEAT.Add(fCurrentBeat);
                    fOFFSET.Add(divisionData.timming.fTotalOffset);
                    fBPM.Add(divisionData.timming.fBPM * fDif);
                    fSCROLL.Add(1 / fDif);
                    fMYSTERY.Add(divisionData.timming.fMystery);
                    iSMOOTH.Add(divisionData.timming.iSmooth);
                    iROWS.Add(divisionData.timming.iRows);
                    iBEATSPLIT.Add(divisionData.timming.iBeatSplit);
                    fSPEED.Add(Math.Abs(divisionData.timming.fSpeed));


                    //excepción de andamiro, el delay/freeze lo toma solamente del primer bloque, los demas no tienen este offset(RARO)
                    float fDelayStop = splitData[sd].divisionData[0].timming.fOffset;

                    if (divisionData.timming.fSpeed > 0)//freeze
                    {
                        fDELAY.Add(0.0f);
                        if (sd == 0)
                            fFREEZE.Add(0.0f);
                        else
                            fFREEZE.Add(fDelayStop / 1000.0f);
                        //fFREEZE.Add(divisionData.timming.fOffset / 1000.0f);
                    }
                    else //offset
                    {
                        fFREEZE.Add(0.0f);
                        if (sd == 0)
                            fDELAY.Add(0.0f);
                        else
                            fDELAY.Add(fDelayStop / 1000.0f);
                        //fDELAY.Add(divisionData.timming.fOffset / 1000.0f);
                    }

                    if (fDelayStop < 0)
                    //if (divisionData.timming.fOffset < 0)
                    {
                        int ms = (int)(60000.0f / (divisionData.timming.fBPM));
                        float beat = (fDelayStop / -1.0f) / ms;
                        //float beat = (divisionData.timming.fOffset / -1.0f) / ms;
                        fWARP.Add(beat * fDif);
                    }
                    else
                        fWARP.Add(0);

                    fDif = 0.125f * (float)divisionData.timming.iRows;
                    fCurrentBeat += fDif;

                    ////para las notas sacar random division data
                    //r = new Random(Guid.NewGuid().GetHashCode());
                    //int iD = iDivision;
                    //if (iDivision == -1)
                    //    iD = r.Next(0, splitData[sd].divisionData.Count);

                    //if (iD < 0)
                    //    iD = 0;
                    //if (iD >= splitData[sd].divisionData.Count)
                    //    iD = splitData[sd].divisionData.Count - 1;
                    //divisionData = splitData[sd].divisionData[iD];
                    if (splitData[sd].divisionData.Count > 1)
                        Console.WriteLine("random block for step " + sFile + ":" + (iD + 1) + "/" + splitData[sd].divisionData.Count);




                    int iCol = 0;
                    bFakeNotesDivision = true;
                    int iNoteCount = 0;
                    for (int s = 0; s < divisionData.step.Count; s++)
                    {
                        Step step = divisionData.step[s];

                        if (iType == 0)
                        {
                            sNOTES.Append(NoteNX10(step, sd));
                        }
                        else if (iType == 1)
                        {
                            if (step.iNote != 33 && step.iNote != 35 && step.iNote != 55 && step.iNote != 59 && step.iNote != 63 && step.iNote != 0)
                            {
                                bFakeNotesDivision = false;
                            }
                            string sNote = NoteNX20(step, sd);
                            if (sNote != "0")
                            {
                                iNoteCount++;
                            }
                            sNOTES.Append(sNote);
                        }
                        else if(iType == 2)
                        {
                            sNOTES.Append(NoteSTF4(step, sd));
                        }


                        iCol++;
                        if (iCol == iCols)
                        {
                            sNOTES.AppendLine("");
                            iCol = 0;
                            iRow++;

                            if ((iCommonBeatSplit * 4) == iRow)
                            {
                                iRow = 0;
                                sNOTES.AppendLine(",");
                            }
                        }
                    }

                    if (iNoteCount == 0)
                    {
                        bFakeNotesDivision = false;
                    }

                    if (splitData[sd].divisionData.Count > 1)
                    {
                        if (bFakeNotesDivision)
                        {
                            iLastDivision = iD;
                            //Console.WriteLine("FFFF:" + (iD + 1));
                        }
                    }

                    if (!bFakeNotesFlag)
                    {
                        bFakeNotesDivision = false;
                    }

                    //iNoteCount = iNoteCount;
                    //if (iNoteCount > 1)
                    //{
                    //    bFakeNotesDivision = false;
                    //}

                }
                int iLeft = (iCommonBeatSplit * 4) - iRow;
                for (int l = 0; l < iLeft; l++)
                    sNOTES.AppendLine(sCol);

                if (bSkin)
                    sw.WriteLine("#PRELOADNOTESKIN:" + sBank1 + "," + sBank2 + ";");
                else
                    sw.WriteLine("#PRELOADNOTESKIN:;");


                sw.WriteLine("#WARPS:");
                for (int o = 1; o < fOFFSET.Count; o++)
                {
                    if (fWARP[o] <= 0)
                        continue;
                    sw.WriteLine(fBEAT[o] + "=" + fWARP[o] + ",");
                }
                sw.WriteLine(";");

                sw.WriteLine("#STOPS:");
                for (int o = 1; o < fOFFSET.Count; o++)
                {
                    if (fFREEZE[o] <= 0)
                        continue;
                    sw.WriteLine(fBEAT[o] + "=" + fFREEZE[o] + ",");
                }
                sw.WriteLine(";");

                sw.WriteLine("#DELAYS:");
                for (int o = 1; o < fOFFSET.Count; o++)
                {
                    if (fDELAY[o] <= 0)
                        continue;
                    sw.WriteLine(fBEAT[o] + "=" + fDELAY[o] + ",");
                }
                sw.WriteLine(";");

                //float fLastBPM = -99;
                sw.WriteLine("#BPMS:");
                for (int o = 0; o < fOFFSET.Count; o++)
                {
                    //if (fLastBPM != fBPM[o])
                    {
                        if (iSMOOTH[o] == 2 || iSMOOTH[o] == 3 || fBPM[o] == 0) //algún flag raro hay en el SMOOTH

                            sw.WriteLine(fBEAT[o] + "=9999999,");
                        else
                            sw.WriteLine(fBEAT[o] + "=" + fBPM[o] + ",");
                    }
                    //fLastBPM = fBPM[o];
                }
                sw.WriteLine(";");

                //float fLastSCROLL = -99;
                sw.WriteLine("#SCROLLS:");
                for (int o = 0; o < fOFFSET.Count; o++)
                {
                    //if (fLastSCROLL != fSCROLL[o])
                    {
                        if (fMYSTERY[o] == 0)
                            sw.WriteLine(fBEAT[o] + "=0,");
                        else
                            sw.WriteLine(fBEAT[o] + "=" + fSCROLL[o] + ",");
                    }
                    //fLastSCROLL = fSCROLL[o];
                }
                sw.WriteLine(";");

                float fLastSPEED = -99;
                sw.WriteLine("#SPEEDS:");
                for (int o = 0; o < fOFFSET.Count; o++)
                {
                    if (fLastSPEED != fSPEED[o])
                    {
                        if (iSMOOTH[o] == 0)
                            sw.WriteLine(fBEAT[o] + "=" + fSPEED[o] + "=1=1,");
                        else
                        {
                            float fLenght = (float)(iROWS[o]) / (float)iCommonBeatSplit;
                            sw.WriteLine(fBEAT[o] + "=" + fSPEED[o] + "=" + fLenght + "=0,");
                        }
                    }
                    fLastSPEED = fSPEED[o];
                }
                sw.WriteLine(";");


                sw.WriteLine("#TIMESIGNATURES:0.000000=4=4;");
                sw.WriteLine("#TICKCOUNTS:0=" + iCommonBeatSplit + ";");
                sw.WriteLine("#COMBOS:0.000000=1;");
                sw.WriteLine("#FAKES:;");
                sw.WriteLine("#LABELS:;");

                sw.WriteLine("#NOTES:");

                bool bRemoveEmpty = true;

                if (bRemoveEmpty)
                {
                    StringBuilder sbNewNotes = new StringBuilder();
                    string[] sSplitNotes = sNOTES.ToString().Split(',');
                    for (int n = 0; n < sSplitNotes.Length; n++)
                    {
                        bool bEmpty = true;
                        string[] sLine = sSplitNotes[n].Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        sLine = sLine.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        for (int l = 0; l < sLine.Length; l++)
                            if (sLine[l] != sCol)
                                bEmpty = false;
                        if (bEmpty)
                            sbNewNotes.AppendLine(sCol);
                        else
                            sbNewNotes.AppendLine(string.Join("\r\n", sLine));

                        if (sSplitNotes.Length - 1 != n)
                            sbNewNotes.AppendLine(",");
                    }
                    sw.WriteLine(sbNewNotes.ToString());
                }
                else
                    sw.WriteLine(sNOTES.ToString());

                sw.WriteLine(";");
            }

            int iTotalItem = 0;
            for (int i = 0; i < iItem.Length; i++)
                iTotalItem += iItem[i];

            if (iTotalItem > 0)
            {
                sFile = Path.ChangeExtension(sFile, ".ini");
                using (StreamWriter sw = new StreamWriter(sFile, true))
                {
                    sw.WriteLine("[TOTALITEM]");
                    for (int i = 0; i < sItem.Length; i++)
                        sw.WriteLine(sItem[i] + "=" + iItem[i]);
                    sw.WriteLine("Total=" + iTotalItem);
                }

            }


            return true;
        }

        bool Read(string sFile, string sDiff = null, int iL = 0)
        {
           // sFile = "STEP.STX";



            bFakeNotesDivision = false;
            iLastDivision = 0;
            

            if (iDivision == 999)
            {
                bFakeNotesFlag = true;
            }
            int iType = -1;//nx10, 1 = nx20, 2 = STF4
            //antes que nada sacar el header...
            {
                FileStream fs = new FileStream(sFile, FileMode.Open);
                List<byte[]> lHeader = new List<byte[]>();
                byte[] bByte;
                //for (int i = 0; i < 4; i++)
                {
                    bByte = new byte[4];
                    fs.Read(bByte, 0, 4);
                    lHeader.Add(bByte);
                }

                if (lHeader[0][0] == 78 && lHeader[0][1] == 88 && lHeader[0][2] == 49 && lHeader[0][3] == 48)
                    iType = 0;
                if (lHeader[0][0] == 78 && lHeader[0][1] == 88 && lHeader[0][2] == 50 && lHeader[0][3] == 48)
                    iType = 1;
                if (lHeader[0][0] == 83 && lHeader[0][1] == 84 && lHeader[0][2] == 70 && lHeader[0][3] == 52)
                    iType = 2;

                fs.Close();
            }
            if (iType == -1)            
                return false;

            for (int i = 0; i < iItem.Length; i++)            
                iItem[i] = 0;

            List<SplitData> splitData = new List<SplitData>();
            int iLevel = 1;
            int iCols = 5;

            if (iType == 0)
            {
                NX10Reader nx10Reader = new NX10Reader();

                NX10File nx10File = nx10Reader.Read(sFile);
                splitData = nx10File.splitdata;
                iLevel = nx10File.iLevel;
                iCols = nx10File.iCol;


                WriteStep(splitData, iCols, iType, sFile, iLevel, sDiff, iL);
            }
            if (iType == 1)
            {
                NX20Reader nx20Reader = new NX20Reader();

                NX20File nx20File = nx20Reader.Read(sFile);
                splitData = nx20File.splitdata;
                iLevel = nx20File.iLevel;
                iCols = nx20File.iCol;

                WriteStep(splitData, iCols, iType, sFile, iLevel, sDiff, iL);
            }
            if (iType == 2)
            {
                STXReader stxReader = new STXReader();

                List<STXFile> stxFile = stxReader.Read(sFile);
                Console.WriteLine();
                foreach (STXFile stx in stxFile)
                {
                    splitData = stx.splitdata;                    
                    iLevel = stx.iLevel;
                    iCols = stx.iCol;
                    sFile = stx.sFile;


                    if (sFile == "PR.NX" || sFile == "LM.NX")
                        continue;
                    if (stx.iTotalStep > 0)
                        WriteStep(splitData, iCols, iType, sFile, iLevel, sDiff, iL);


                }

                //splitData = stxFile.splitdata;
                //iLevel = stxFile.iLevel;
                //iCols = stxFile.iCol;
            }


            //if (splitData.Count == 0)
            //    return false;

            //bSkin = false;

            //using (StreamWriter sw = new StreamWriter("STEP.SSC", true))
            //{
            //    string sDifficulty = "Edit";
               
            //    if (iCols < 5)
            //        return false;

            //    if (iType == 0)
            //        Console.WriteLine("\t NX10...");
            //    if (iType == 1)
            //        Console.WriteLine("\t NX20...");

            //    string sStepType = "pump-single";
            //    if (iCols == 6)
            //        sStepType = "pump-halfdouble";
            //    if (iCols == 10)
            //        sStepType = "pump-double";

            //    if (sFile.ToUpper() == "NO.NX")
            //    {
            //        sDifficulty = "Beginner";
            //        iLevel = 1;
            //    }
            //    else if (sFile.ToUpper() == "HD.NX")
            //    {
            //        sDifficulty = "Easy";
            //        iLevel = 2;
            //    }
            //    else if (sFile.ToUpper() == "CR.NX")
            //    {
            //        sDifficulty = "Medium";
            //        iLevel = 3;
            //    }
            //    else if (sFile.ToUpper() == "FR.NX")
            //    {
            //        sDifficulty = "Beginner";
            //        iLevel = 1;
            //    }
            //    else if (sFile.ToUpper() == "NM.NX")
            //    {
            //        sDifficulty = "Easy";
            //        iLevel = 2;
            //    }
            //    else if (sFile.ToUpper() == "HF.NX")
            //    {
            //        sDifficulty = "Beginner";
            //        iLevel = 1;
            //    }

            //    string sChartName = "";
            //    if (sDiff != null)
            //    {
            //        sDifficulty = sDiff;
            //        iLevel = iL;
            //        sChartName = "STEP" + iL;
            //    }

            //    sw.WriteLine("//---------------pump-single - " + sFile + "----------------");
            //    sw.WriteLine("#NOTEDATA:;");
            //    sw.WriteLine("#CHARTNAME:" + sChartName + ";");
            //    sw.WriteLine("#STEPSTYPE:" + sStepType + ";");
            //    sw.WriteLine("#DESCRIPTION:" + sFile + ";");
            //    sw.WriteLine("#LABELTYPE:NORMAL;");
            //    //sw.WriteLine("#PRELOADNOTESKIN:;");
            //    sw.WriteLine("#SEPARATENOTES:NO;");
            //    sw.WriteLine("#PLAYERS:1;");
            //    sw.WriteLine("#QUESTDESC:;");
            //    sw.WriteLine("#QUESTMODS:;");
            //    sw.WriteLine("#QUESTBARINITIAL:0.500;");
            //    sw.WriteLine("#QUESTBARLEVEL:0;");
            //    sw.WriteLine("#CHARTSTYLE:;");
            //    sw.WriteLine("#DIFFICULTY:" + sDifficulty + ";");
            //    sw.WriteLine("#METER:" + iLevel.ToString() + ";");
            //    sw.WriteLine("#RADARVALUES:;");
            //    sw.WriteLine("#CREDIT:HANS NX20;");
            //    sw.WriteLine("#OFFSET:" + (splitData[0].divisionData[0].timming.fOffset / -1000.0f).ToString() + ";");

            //    List<float> fOFFSET = new List<float>();
            //    List<float> fBPM = new List<float>();
            //    List<float> fBEAT = new List<float>();
            //    List<float> fDELAY = new List<float>();
            //    List<float> fFREEZE = new List<float>();
            //    List<int> iBEATSPLIT = new List<int>();//no se usa
            //    List<float> fSPEED = new List<float>();
            //    List<float> fSCROLL = new List<float>();
            //    List<float> fMYSTERY = new List<float>();
            //    List<int> iSMOOTH = new List<int>();
            //    List<int> iROWS = new List<int>();
            //    List<float> fWARP = new List<float>();

            //    string sCol = null;
            //    for (int c = 0; c < iCols; c++)
            //        sCol += "0";

            //    float fCurrentBeat = 0;

            //    StringBuilder sNOTES = new StringBuilder();

            //    int iRow = 0;
            //    Random r;
            //    for (int sd = 0; sd < splitData.Count; sd++)
            //    {
                    
            //        //para el timming data sacar el primer division, andamiro solamente le pone timming data al primer step(NI IDEA POR QUE)
            //        //para las notas sacar random division data
            //        r = new Random(Guid.NewGuid().GetHashCode());
            //        int iD = iDivision;
            //        if (iDivision < -1 || iDivision == 999)                    
            //            iDivision = -1;

                    

            //        if (!bFakeNotesDivision)
            //        {
            //            if (iDivision == -1)
            //                iD = r.Next(0, splitData[sd].divisionData.Count);
            //        }
            //        else
            //        {
            //            iD = iLastDivision;
            //        }

            //        if (iD < 0)
            //            iD = 0;
            //        if (iD >= splitData[sd].divisionData.Count)
            //            iD = splitData[sd].divisionData.Count - 1;

            //        //if (iDivision == 999)
            //        //{
            //        //    iD = iAuxDivision;
            //        //    //iAuxDivision++;
            //        //}



            //        DivisionData divisionData = splitData[sd].divisionData[iD];
            //        float fDif = (float)divisionData.timming.iBeatSplit / (float)iCommonBeatSplit;

            //        fBEAT.Add(fCurrentBeat);
            //        fOFFSET.Add(divisionData.timming.fTotalOffset);
            //        fBPM.Add(divisionData.timming.fBPM * fDif);
            //        fSCROLL.Add(1 / fDif);
            //        fMYSTERY.Add(divisionData.timming.fMystery);
            //        iSMOOTH.Add(divisionData.timming.iSmooth);
            //        iROWS.Add(divisionData.timming.iRows);
            //        iBEATSPLIT.Add(divisionData.timming.iBeatSplit);
            //        fSPEED.Add(Math.Abs(divisionData.timming.fSpeed));


            //        //excepción de andamiro, el delay/freeze lo toma solamente del primer bloque, los demas no tienen este offset(RARO)
            //        float fDelayStop = splitData[sd].divisionData[0].timming.fOffset;

            //        if (divisionData.timming.fSpeed > 0 )//freeze
            //        {
            //            fDELAY.Add(0.0f);
            //            if (sd == 0)
            //                fFREEZE.Add(0.0f);
            //            else
            //                fFREEZE.Add(fDelayStop / 1000.0f);                        
            //                //fFREEZE.Add(divisionData.timming.fOffset / 1000.0f);
            //        }
            //        else //offset
            //        {
            //            fFREEZE.Add(0.0f);
            //            if (sd == 0)
            //                fDELAY.Add(0.0f);
            //            else
            //                fDELAY.Add(fDelayStop / 1000.0f);
            //                //fDELAY.Add(divisionData.timming.fOffset / 1000.0f);
            //        }

            //        if (fDelayStop < 0)
            //        //if (divisionData.timming.fOffset < 0)
            //        {
            //            int ms = (int)(60000.0f / (divisionData.timming.fBPM));
            //            float beat = (fDelayStop / -1.0f) / ms;
            //            //float beat = (divisionData.timming.fOffset / -1.0f) / ms;
            //            fWARP.Add(beat * fDif);
            //        }
            //        else
            //            fWARP.Add(0);

            //        fDif = 0.125f * (float)divisionData.timming.iRows;
            //        fCurrentBeat += fDif;

            //        ////para las notas sacar random division data
            //        //r = new Random(Guid.NewGuid().GetHashCode());
            //        //int iD = iDivision;
            //        //if (iDivision == -1)
            //        //    iD = r.Next(0, splitData[sd].divisionData.Count);

            //        //if (iD < 0)
            //        //    iD = 0;
            //        //if (iD >= splitData[sd].divisionData.Count)
            //        //    iD = splitData[sd].divisionData.Count - 1;
            //        //divisionData = splitData[sd].divisionData[iD];
            //        if (splitData[sd].divisionData.Count > 1)                    
            //            Console.WriteLine("random block for step " + sFile + ":" + (iD + 1) + "/" + splitData[sd].divisionData.Count);


              

            //        int iCol = 0;
            //        bFakeNotesDivision = true;
            //        int iNoteCount = 0;
            //        for (int s = 0; s < divisionData.step.Count; s++)
            //        {
            //            Step step = divisionData.step[s];

            //            if (iType == 0)
            //            {
            //                sNOTES.Append(NoteNX10(step, sd));
            //            }
            //            else if (iType == 1)
            //            {
            //                if (step.iNote != 33 && step.iNote != 35 && step.iNote != 55 && step.iNote != 59 && step.iNote != 63 && step.iNote != 0)
            //                {
            //                    bFakeNotesDivision = false;
            //                }
            //                string sNote = NoteNX20(step, sd);
            //                if (sNote != "0")
            //                {
            //                    iNoteCount++;
            //                }
            //                sNOTES.Append(sNote);
            //            }
                        

            //            iCol++;
            //            if (iCol == iCols)
            //            {
            //                sNOTES.AppendLine("");
            //                iCol = 0;
            //                iRow++;

            //                if ((iCommonBeatSplit * 4) == iRow)
            //                {
            //                    iRow = 0;
            //                    sNOTES.AppendLine(",");
            //                }
            //            }
            //        }

            //        if (iNoteCount == 0)
            //        {
            //            bFakeNotesDivision = false;
            //        }

            //        if (splitData[sd].divisionData.Count > 1)
            //        {
            //            if (bFakeNotesDivision)
            //            {
            //                iLastDivision = iD;
            //                //Console.WriteLine("FFFF:" + (iD + 1));
            //            }
            //        }

            //        if (!bFakeNotesFlag)
            //        {
            //            bFakeNotesDivision = false;
            //        }

            //        //iNoteCount = iNoteCount;
            //        //if (iNoteCount > 1)
            //        //{
            //        //    bFakeNotesDivision = false;
            //        //}

            //    }
            //    int iLeft = (iCommonBeatSplit * 4) - iRow;
            //    for (int l = 0; l < iLeft; l++)                
            //        sNOTES.AppendLine(sCol);

            //    if (bSkin)
            //        sw.WriteLine("#PRELOADNOTESKIN:" + sBank1 + "," + sBank2 + ";");
            //    else
            //        sw.WriteLine("#PRELOADNOTESKIN:;");


            //    sw.WriteLine("#WARPS:");
            //    for (int o = 1; o < fOFFSET.Count; o++)
            //    {
            //        if (fWARP[o] <= 0)
            //            continue;
            //        sw.WriteLine(fBEAT[o] + "=" + fWARP[o] + ",");
            //    }
            //    sw.WriteLine(";");

            //    sw.WriteLine("#STOPS:");
            //    for (int o = 1; o < fOFFSET.Count; o++)
            //    {
            //        if (fFREEZE[o] <= 0)
            //            continue;
            //        sw.WriteLine(fBEAT[o] + "=" + fFREEZE[o] + ",");
            //    }
            //    sw.WriteLine(";");

            //    sw.WriteLine("#DELAYS:");
            //    for (int o = 1; o < fOFFSET.Count; o++)
            //    {
            //        if (fDELAY[o] <= 0)
            //            continue;
            //        sw.WriteLine(fBEAT[o] + "=" + fDELAY[o] + ",");
            //    }
            //    sw.WriteLine(";");

            //    //float fLastBPM = -99;
            //    sw.WriteLine("#BPMS:");
            //    for (int o = 0; o < fOFFSET.Count; o++)
            //    {
            //        //if (fLastBPM != fBPM[o])
            //        {
            //            if (iSMOOTH[o] == 2 || iSMOOTH[o] == 3 || fBPM[o] == 0) //algún flag raro hay en el SMOOTH

            //                sw.WriteLine(fBEAT[o] + "=9999999,");
            //            else
            //                sw.WriteLine(fBEAT[o] + "=" + fBPM[o] + ",");
            //        }
            //        //fLastBPM = fBPM[o];
            //    }
            //    sw.WriteLine(";");

            //    //float fLastSCROLL = -99;
            //    sw.WriteLine("#SCROLLS:");
            //    for (int o = 0; o < fOFFSET.Count; o++)
            //    {
            //        //if (fLastSCROLL != fSCROLL[o])
            //        {
            //            if (fMYSTERY[o] == 0)
            //                sw.WriteLine(fBEAT[o] + "=0,");
            //            else
            //                sw.WriteLine(fBEAT[o] + "=" + fSCROLL[o] + ",");
            //        }
            //        //fLastSCROLL = fSCROLL[o];
            //    }
            //    sw.WriteLine(";");

            //    float fLastSPEED = -99;
            //    sw.WriteLine("#SPEEDS:");
            //    for (int o = 0; o < fOFFSET.Count; o++)
            //    {
            //        if (fLastSPEED != fSPEED[o])
            //        {
            //            if (iSMOOTH[o] == 0)
            //                sw.WriteLine(fBEAT[o] + "=" + fSPEED[o] + "=1=1,");
            //            else
            //            {
            //                float fLenght = (float)(iROWS[o]) / (float)iCommonBeatSplit;
            //                sw.WriteLine(fBEAT[o] + "=" + fSPEED[o] + "=" + fLenght + "=0,");
            //            }
            //        }
            //        fLastSPEED = fSPEED[o];
            //    }
            //    sw.WriteLine(";");


            //    sw.WriteLine("#TIMESIGNATURES:0.000000=4=4;");
            //    sw.WriteLine("#TICKCOUNTS:0=" + iCommonBeatSplit + ";");
            //    sw.WriteLine("#COMBOS:0.000000=1;");
            //    sw.WriteLine("#FAKES:;");
            //    sw.WriteLine("#LABELS:;");

            //    sw.WriteLine("#NOTES:");

            //    bool bRemoveEmpty = true;

            //    if (bRemoveEmpty)
            //    {
            //        StringBuilder sbNewNotes = new StringBuilder();
            //        string[] sSplitNotes = sNOTES.ToString().Split(',');
            //        for (int n = 0; n < sSplitNotes.Length; n++)
            //        {
            //            bool bEmpty = true;
            //            string[] sLine = sSplitNotes[n].Split(new string[] { "\r\n" }, StringSplitOptions.None);
            //            sLine = sLine.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            //            for (int l = 0; l < sLine.Length; l++)
            //                if (sLine[l] != sCol)
            //                    bEmpty = false;
            //            if (bEmpty)
            //                sbNewNotes.AppendLine(sCol);
            //            else
            //                sbNewNotes.AppendLine(string.Join("\r\n", sLine));

            //            if (sSplitNotes.Length - 1 != n)
            //                sbNewNotes.AppendLine(",");
            //        }
            //        sw.WriteLine(sbNewNotes.ToString());
            //    }
            //    else                
            //        sw.WriteLine(sNOTES.ToString());

            //    sw.WriteLine(";");
            //}

            //int iTotalItem = 0;
            //for (int i = 0; i < iItem.Length; i++)
            //    iTotalItem += iItem[i];

            //if (iTotalItem > 0)
            //{
            //    sFile = Path.ChangeExtension(sFile, ".ini");
            //    using (StreamWriter sw = new StreamWriter(sFile, true))
            //    {
            //        sw.WriteLine("[TOTALITEM]");
            //        for (int i = 0; i < sItem.Length; i++)
            //            sw.WriteLine(sItem[i] + "=" + iItem[i]);
            //        sw.WriteLine("Total=" + iTotalItem);
            //    }
                
            //}

            

            return true;
            //Console.ReadKey();
        }
    }
}
