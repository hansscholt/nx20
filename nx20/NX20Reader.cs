using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace nx20
{
    class NX20File
    {
        public List<SplitData> splitdata;
        public int iCol;
        public int iLevel;
    }
    
    class NX20Reader
    {
        public bool Signature(string sPath)
        {
            bool toReturn = false;
            FileStream fs = new FileStream(sPath, FileMode.Open);

            List<byte[]> lHeader = new List<byte[]>();
            byte[] bByte;
            //for (int i = 0; i < 4; i++)
            {
                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                lHeader.Add(bByte);
            }

            if ((lHeader[0][0] == 78 &&
                lHeader[0][1] == 88 &&
                lHeader[0][2] == 50 &&
                lHeader[0][3] == 48))
                toReturn = true;


            fs.Close();

            return toReturn;
        }

        public NX20File Read(string sPath)
        {
            NX20File nx20File = new NX20File();

            FileStream fs = new FileStream(sPath, FileMode.Open);

            //int[] iBytesHeader = new int[] {
            //    4,//firma
            //    4,//nada ?
            //    4,//cols                INT32 - Little Endian (DCBA)
            //    8,//nada ?
            //    4,//splits              INT32 - Little Endian (DCBA)
            //    8,//nada ?                
            //};

            int[] iBytesHeaderPrime = new int[] {
                4,//firma
                4,//nada ?
                4,//cols                INT32 - Little Endian (DCBA)
                4,//nada ?
                4,//basura?             INT32 - Little Endian (DCBA)
                //4,//splits              INT32 - Little Endian (DCBA)
                //8,//nada ?                
            };

            int[] iBytesStep = new int[]
            {
                //4,//division            INT32 - Little Endian (DCBA)
                4,//total offset        Float - Little Endian (DCBA)
                4,//bpm                 Float - Little Endian (DCBA)
                //1,//0.25, 0.5 ?         Float - Little Endian (DCBA)
                //1,//0.25, 0.5 ?         Float - Little Endian (DCBA)
                //1,//0.25, 0.5 ?         Float - Little Endian (DCBA)
                //1,//0.25, 0.5 ?         Float - Little Endian (DCBA)
                4,//mystery             Float - Little Endian (DCBA)
                4,//this block offset   Float - Little Endian (DCBA)
                4,//speed               Float - Little Endian (DCBA)
                1,//beatsplit
                1,//beat per measure
                1,//smooth
                1,//?
                4,//division INFO
                /*
                 * cuando existen bloques de divsion INFO se usan 8 bytes por info
                 * los primeros 4 indican el tipo de score, perfect,great,good,bad,miss,g,w,a,b,c
                 * los otros 4 se ocupan de 2 en 2, 2 primeros para indicar el mínimo score y 2 últimos para el máximo score
                 * ej: 
                 * 0A000000 //score tipo 10
                 * 0A006400 //entre 10 y 100 
                 */
                //4,//rows              INT32 - Little Endian (DCBA)
            };


            List<byte[]> lHeader = new List<byte[]>();
            byte[] bByte;
            for (int i = 0; i < iBytesHeaderPrime.Length; i++)
            {
                bByte = new byte[iBytesHeaderPrime[i]];
                fs.Read(bByte, 0, iBytesHeaderPrime[i]);
                lHeader.Add(bByte);
            }

            if (!(lHeader[0][0] == 78 &&
                lHeader[0][1] == 88 &&
                lHeader[0][2] == 50 &&
                lHeader[0][3] == 48))
                return new NX20File();

            int iCol = BitConverter.ToInt32(lHeader[2], 0);         //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            nx20File.iCol = iCol;

            //nx20 prime
            int iTrash= BitConverter.ToInt32(lHeader[4], 0); ;

            int iLevel = 0;

            List<byte[][]> byteTrash = new List<byte[][]>();

            List<int[]> lTrash = new List<int[]>();
            for (int t = 0; t < iTrash; t++)
            {
                byte[] b1;
                byte[] b2;
                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                b1 = bByte;
                int iCode = BitConverter.ToInt32(bByte, 0);
                fs.Read(bByte, 0, 4);
                b2 = bByte;
                int iData = BitConverter.ToInt32(bByte, 0);

                if (iCode == 1001)
                    iLevel = iData;

                byteTrash.Add(new byte[][] { b1, b2 });
                lTrash.Add(new int[] { iCode, iData });
            }
            nx20File.iLevel = iLevel;

            bByte = new byte[4];
            fs.Read(bByte, 0, 4);
            int iSplit = BitConverter.ToInt32(bByte, 0);       //>>>>>>>>>>>>>>>>>>>>>>>>>>>>


            //fs.Position += 8;//nada
            

            bByte = new byte[4];
            fs.Read(bByte, 0, 4);
            int iSomethingFlag = BitConverter.ToInt32(bByte, 0);
            if (iSomethingFlag == 0)
                fs.Position += 4;//nada
            else if (iSomethingFlag == 1 || iSomethingFlag == 128 || iSomethingFlag == 65 || iSomethingFlag == 129)
            {
                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                Console.WriteLine("UKNOWN DIVISION FLAG: " + iSomethingFlag.ToString());
                int iBytesToSkip = BitConverter.ToInt32(bByte, 0);//????????????
                fs.Position += (8 * iBytesToSkip);//nada
            }

            //bByte = new byte[8];
            //fs.Read(bByte, 0, 8);

            List<SplitData> splitData = new List<SplitData>();
            
            for (int s = 0; s < iSplit; s++)
            {
                SplitData split = new SplitData();
                byte[] bBlock = new byte[4];
                fs.Read(bBlock, 0, 4);
                int iDivision = BitConverter.ToInt32(bBlock, 0);    //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                split.divisionData = new List<DivisionData>();
                for (int d = 0; d < iDivision; d++)
                {
                    //byte[] bByte;

                    List<byte[]> lStep = new List<byte[]>();
                    DivisionData divisionData = new DivisionData();
                    for (int i = 0; i < iBytesStep.Length; i++)
                    {
                        bByte = new byte[iBytesStep[i]];
                        fs.Read(bByte, 0, iBytesStep[i]);
                        lStep.Add(bByte);
                    }

                    float fTotalOffset = BitConverter.ToSingle(lStep[0], 0); //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    float fBPM = BitConverter.ToSingle(lStep[1], 0);        //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    float fMystery = BitConverter.ToSingle(lStep[2], 0);     //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    float fOffset = BitConverter.ToSingle(lStep[3], 0);     //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    float fSpeed = BitConverter.ToSingle(lStep[4], 0);      //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    int iBeatSplit = (int)(lStep[5][0]);                    //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    int iBeatPerMeasure = (int)(lStep[6][0]);               //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    int iSmooth = (int)(lStep[7][0]);               //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    int iDivisionInfo = (int)(lStep[9][0]);                //>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    divisionData.divisionInfo = new List<DivisionInfo>();

                    if (iDivisionInfo == 0)
                    {
                        DivisionInfo divisionInfo = new DivisionInfo();
                        divisionInfo.iScore = -1;
                        divisionInfo.iMin = 0;
                        divisionInfo.iMax = 0;
                        divisionData.divisionInfo.Add(divisionInfo);
                    }
                    for (int di = 0; di < iDivisionInfo; di++)
                    {
                        DivisionInfo divisionInfo = new DivisionInfo();
                        bByte = new byte[4];
                        fs.Read(bByte, 0, 4);
                        divisionInfo.iScore = BitConverter.ToInt32(bByte, 0);
                        bByte = new byte[2];
                        fs.Read(bByte, 0, 2);
                        divisionInfo.iMin = BitConverter.ToInt16(bByte, 0);
                        bByte = new byte[2];
                        fs.Read(bByte, 0, 2);
                        divisionInfo.iMax = BitConverter.ToInt16(bByte, 0);
                        divisionData.divisionInfo.Add(divisionInfo);
                    }

                    bByte = new byte[4];
                    fs.Read(bByte, 0, 4);
                    int iRows = BitConverter.ToInt32(bByte, 0);             //>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    divisionData.timming.fTotalOffset = fTotalOffset;
                    divisionData.timming.fBPM = fBPM;
                    divisionData.timming.fMystery = fMystery;
                    divisionData.timming.fOffset = fOffset;
                    divisionData.timming.fSpeed = fSpeed;
                    divisionData.timming.iBeatSplit = iBeatSplit;
                    divisionData.timming.iBeatPerMeasure = iBeatPerMeasure;
                    divisionData.timming.iSmooth = iSmooth;
                    divisionData.timming.iRows = iRows;

                    divisionData.step = new List<Step>();
                    for (int b = 0; b < iRows; b++)
                    {
                        Step step = new Step();
                        step.iRow = b;
                        bByte = new byte[4];
                        fs.Read(bByte, 0, 4);
                        if (bByte[0] == 128)
                        {
                            //converter
                            for (int c = 0; c < iCol; c++)
                            {
                                step.iCol = c;
                                divisionData.step.Add(step);
                            }
                        }
                        else
                        {
                            fs.Position -= 4;
                            for (int c = 0; c < iCol; c++)
                            {
                                bByte = new byte[4];
                                fs.Read(bByte, 0, 4);
                                step.iCol = c;

                                step.iNote = bByte[0];
                                step.iLayer = bByte[1];
                                step.iPlayer = bByte[2];
                                step.iSpecial = bByte[3];

                                //converter
                                //if (step.iNote != 0)
                                    divisionData.step.Add(step);
                            }
                        }
                    }

                    //divisionData.iBeatSplit = divisionData.timming.iBeatSplit;
                    divisionData.fSpace = 48.0f / divisionData.timming.iBeatSplit;
                    divisionData.iCurrentDivision = d;
                    split.divisionData.Add(divisionData);

                    split.fHeight = split.divisionData[d].timming.iRows * 48 / split.divisionData[d].timming.iBeatSplit;
                    split.iCol = iCol;
                    //split.iLevel = iLevel;

                    //split.fSpace = 48.0f / split.divisionData[d].timming.iBeatSplit;
                    //Console.WriteLine(split.fSpace);
                    split.iCurrentSplit = s;
                    //split.iBeatSplit = split.divisionData[d].timming.iBeatSplit;
                }
                splitData.Add(split);

                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                iSomethingFlag = BitConverter.ToInt32(bByte, 0);
                if (iSomethingFlag == 0)
                    fs.Position += 4;//nada
                else if(iSomethingFlag == 1 || iSomethingFlag == 128 || iSomethingFlag == 65 || iSomethingFlag == 129)
                {
                    bByte = new byte[4];
                    fs.Read(bByte, 0, 4);
                    Console.WriteLine("UKNOWN DIVISION FLAG: " + iSomethingFlag.ToString());
                    int iBytesToSkip = BitConverter.ToInt32(bByte, 0);//????????????
                    fs.Position += (8 * iBytesToSkip);//nada
                }
                else
                {
                    fs.Position -= 4;   //end of file?
                }
            }


            

            nx20File.splitdata = splitData;

            bool dDivisionInfo = false;
            bool bTrash = false;
            foreach (SplitData splitdata in nx20File.splitdata)
            {
                foreach (DivisionData divisionData in splitdata.divisionData)
                {
                    foreach (DivisionInfo divisioninfo in divisionData.divisionInfo)
                    {
                        if (divisioninfo.iScore != -1)
                        {
                            dDivisionInfo = true;
                            break;
                        }
                    }
                }
            }
            if (lTrash.Count > 0)            
                bTrash = true;





            sPath = Path.ChangeExtension(sPath, ".ini");
            if (File.Exists(sPath))
                File.Delete(sPath);

            string sMOD = null;

            List<int[]> lMissionInfo = new List<int[]>();
            if (dDivisionInfo || bTrash)
            {
                using (StreamWriter sw = new StreamWriter(sPath, false))
                {
                    if (bTrash)
                    {
                        if (lTrash.Count > 0)
                        {
                            sw.WriteLine("[METADATA]");
                            for (int t = 0; t < lTrash.Count; t++)
                            {
                                sw.WriteLine(lTrash[t][0] + "=" + lTrash[t][1]);
                                if (lTrash[t][0] == 0)
                                {                                    
                                    sw.WriteLine("#XSPEED=" + BitConverter.ToSingle(byteTrash[t][1], 0));
                                    sMOD += BitConverter.ToSingle(byteTrash[t][1], 0) + "x,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 83)
                                {
                                    sw.WriteLine("#BREAK=" + (BitConverter.ToInt32(byteTrash[t][1], 0) == 1 ? "YES": "NO"));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 1101)
                                {
                                    sw.WriteLine("#F1 METER=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 1201)
                                {
                                    sw.WriteLine("#F2 METER=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 1301)
                                {
                                    sw.WriteLine("#F3 METER=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 1401)
                                {
                                    sw.WriteLine("#F4 METER=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 1)
                                {
                                    sw.WriteLine("#EARTHWORM=YES");
                                    sMOD += "EW,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 2)
                                {
                                    //velocidades
                                    if (lTrash[t][1] == 1)
                                    {
                                        sw.WriteLine("#ACCELERATION=YES");
                                        sMOD += "AC,";
                                    }
                                    else if (lTrash[t][1] == 2)
                                    {
                                        sw.WriteLine("#DECCELERATION=YES");
                                        sMOD += "DC,";
                                    }
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 16)
                                {
                                    if (lTrash[t][1] == 1)
                                    {
                                        sw.WriteLine("#VANISH=YES");
                                        sMOD += "V,";
                                    }
                                    else if (lTrash[t][1] == 2)
                                    {
                                        sw.WriteLine("#APPEAR=YES");
                                        sMOD += "AP,";
                                    }
                                    else if (lTrash[t][1] == 3)
                                    {
                                        sw.WriteLine("#NONSTEP=YES");
                                        sMOD += "NS,";
                                    }
                                    else
                                        sw.WriteLine("#VISIBLE=???????");
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 17)
                                {
                                    sw.WriteLine("#FREEDOM=YES");
                                    sMOD += "FD,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 18)
                                {
                                    sw.WriteLine("#FLASH=YES");
                                    sMOD += "FL,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 19)
                                {
                                    sw.WriteLine("#RANDOMSKIN=YES");
                                    sMOD += "RSK,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 20)
                                {
                                    sw.WriteLine("#BGAOFF=YES");
                                    sMOD += "BGAOFF,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 21)
                                {
                                    sw.WriteLine("#XMODE=YES");
                                    sMOD += "X,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 22)
                                {
                                    sw.WriteLine("#NXMODE=YES");
                                    sMOD += "NXMODE,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 32)
                                {
                                    if (lTrash[t][1] == 1)
                                    {
                                        sw.WriteLine("#UNDERATTACK=YES");
                                        sMOD += "UA,";
                                    }
                                    else if (lTrash[t][1] == 2)
                                    {
                                        sw.WriteLine("#DROP=YES");
                                        sMOD += "DR,";
                                    }
                                    else if (lTrash[t][1] == 3)
                                    {
                                        sw.WriteLine("#UNDERATTACK=YES");
                                        sw.WriteLine("#DROP=YES");
                                        sMOD += "UA,DR,";
                                    }
                                    else
                                        sw.WriteLine("#PATH=???????");
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 33)
                                {
                                    if (lTrash[t][1] == 1)
                                    {
                                        sw.WriteLine("#SINK=YES");
                                        sMOD += "SI,";
                                    }
                                    else if (lTrash[t][1] == 2)
                                    {
                                        sw.WriteLine("#RISE=YES");
                                        sMOD += "RI,";
                                    }
                                    else
                                        sw.WriteLine("#ZZZZ=???????");
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 34)
                                {
                                    sw.WriteLine("#SNAKE=YES");
                                    sMOD += "SN,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 35)
                                {
                                    sw.WriteLine("#ZIGZAG=YES");
                                    sMOD += "ZZ,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 64)
                                {
                                    sw.WriteLine("#SEPARATERESULT:YES");
                                    sw.WriteLine();

                                }
                                else if (lTrash[t][0] == 65)
                                {
                                    if (lTrash[t][1] == 400)
                                    {
                                        sw.WriteLine("#JUDGE=EJ");
                                        sMOD += "EJ,";
                                        sw.WriteLine();
                                    }
                                    else if (lTrash[t][1] == 500)
                                    {
                                        sw.WriteLine("#JUDGE=NJ");
                                        //sMOD += "HJ,";
                                        sw.WriteLine();
                                    }
                                    else if (lTrash[t][1] == 600)
                                    {
                                        sw.WriteLine("#JUDGE=HJ");
                                        sMOD += "HJ,";
                                        sw.WriteLine();
                                    }
                                    else if (lTrash[t][1] == 701)
                                    {
                                        sw.WriteLine("#JUDGE=VJ");
                                        sMOD += "VJ,";
                                        sw.WriteLine();
                                    }
                                    else if (lTrash[t][1] == 754)
                                    {
                                        sw.WriteLine("#JUDGE=XJ");
                                        sMOD += "XJ,";
                                        sw.WriteLine();
                                    }
                                    else
                                    {
                                        sw.WriteLine("#JUDGE=??????????");
                                        sw.WriteLine();
                                    }
                                }
                                else if (lTrash[t][0] == 66)
                                {
                                    sw.WriteLine("#REVERSEJUDGE=YES");
                                    sMOD += "RG,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 67)
                                {
                                    sw.WriteLine("#JUDGEHIDDEN=YES");
                                    sMOD += "JH,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 68)
                                {
                                    sw.WriteLine("#JUDGE NOTE(JN)=YES");
                                    sMOD += "JN,";
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 81)
                                {
                                    sw.WriteLine("#MAXBAR=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 82)
                                {
                                    sw.WriteLine("#INITIALBAR=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                else if (lTrash[t][0] == 900 || lTrash[t][0] == 901 || 
                                        lTrash[t][0] == 902 || lTrash[t][0] == 903 ||
                                        lTrash[t][0] == 904 || lTrash[t][0] == 905)
                                {
                                    string sSkin = null;
                                    switch (lTrash[t][1])
                                    {
                                        case 0: sSkin = "FIESTA"; break;
                                        case 1: sSkin = "FLOWER"; break;
                                        case 2: sSkin = "OLD"; break;
                                        case 3: sSkin = "EASY"; break;
                                        case 4: sSkin = "SLIME"; break;
                                        case 5: sSkin = "MUSIC"; break;
                                        case 6: sSkin = "CANON"; break;
                                        case 7: sSkin = "POKER"; break;
                                        case 8: sSkin = "NX"; break;
                                        case 9: sSkin = "SHEEP"; break;
                                        case 10: sSkin = "HORSE"; break;
                                        case 11: sSkin = "DOG"; break;
                                        case 12: sSkin = "GIRL"; break;
                                        case 13: sSkin = "FIRE"; break;
                                        case 14: sSkin = "ICE"; break;
                                        case 15: sSkin = "WIND"; break;
                                        case 16: sSkin = "PERFOR1"; break;
                                        case 17: sSkin = "PERFOR2"; break;
                                        case 18: sSkin = "PERFOR3"; break;
                                        case 19: sSkin = "NXA"; break;
                                        case 20: sSkin = "NX2"; break;
                                        case 21: sSkin = "LIGHTING"; break;
                                        case 22: sSkin = "DRUM"; break;
                                        case 23: sSkin = "MISSILE"; break;
                                        case 24: sSkin = "AADMB"; break;
                                        case 25: sSkin = "AADMR"; break;
                                        case 26: sSkin = "AADMY"; break;
                                        case 27: sSkin = "SOCCER"; break;
                                        case 28: sSkin = "REBIRTH"; break;
                                        case 29: sSkin = "BASIC"; break;
                                        case 30: sSkin = "FIESTA"; break;//PRIME1
                                        case 31: sSkin = "FIESTA2"; break;//PRIME1
                                        case 254: sSkin = "RANDOMSKIN"; break;//PRIME1
                                        default: sSkin = "??"; break;
                                    }
                                    if (lTrash[t][0] == 900)
                                    {
                                        sw.WriteLine("#INITSKIN=" + sSkin);
                                        sMOD += sSkin.ToLower() + ",";
                                        sw.WriteLine();

                                    }
                                    else
                                    {
                                        sw.WriteLine("#ALTERNATESKIN=" + sSkin);
                                        sw.WriteLine();
                                    }
                                    
                                }
                                else if (lTrash[t][0] == 82)
                                {
                                    sw.WriteLine("#INITIALBAR=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                    sw.WriteLine();
                                }
                                //else if (lTrash[t][0] == 1002)
                                //{
                                //    sw.WriteLine("#PLAYERS=" + BitConverter.ToInt32(byteTrash[t][1], 0));
                                //    sw.WriteLine();
                                //}
                                ////////////////NIVEL F1 INFO MISSION DATA
                                else if(lTrash[t][0] == 1102)
                                    lMissionInfo.Add(new int[]{ lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) } );
                                else if (lTrash[t][0] == 66638)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1103)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 66639)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 132175)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 197711)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 263247)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1150)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1203)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                ////////////////NIVEL F2 INFO MISSION DATA
                                else if (lTrash[t][0] == 66739)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 132275)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 197811)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 263347)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1250)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1303)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                ////////////////NIVEL F3 INFO MISSION DATA
                                else if (lTrash[t][0] == 66839)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 132375)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 197911)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 263447)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1350)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1403)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                ////////////////NIVEL F4 INFO MISSION DATA
                                else if (lTrash[t][0] == 66939)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 132475)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 198011)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 263547)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                else if (lTrash[t][0] == 1450)
                                    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                //else if (lTrash[t][0] == 1403)
                                //    lMissionInfo.Add(new int[] { lTrash[t][0], BitConverter.ToInt32(byteTrash[t][1], 0) });
                                //el último goal es lenght - 4


                            }
                            sw.WriteLine();
                        }
                        if (sMOD == null)
                            sw.WriteLine("#MODARRAY:");
                        else
                            sw.WriteLine("#MODARRAY:" + sMOD.Remove(sMOD.Length - 1));

                        sw.WriteLine();

                        
                        

                        //fs.Position -= 8;
                        int iLast = 0;
                        foreach (int[] mTrash in lMissionInfo)
                        {
                            //int iDif = mTrash[1] - iLast;
                            iLast = mTrash[1] - iLast;
                            bByte = new byte[iLast];
                            fs.Read(bByte, 0, iLast);
                            if (mTrash[0] == 1102)
                                sw.WriteLine("#MISSION NAME:\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 66638)
                                sw.WriteLine("#SONG TITLE:\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1103)
                                sw.WriteLine("#SONG TITLE KR:\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 66639)
                                sw.WriteLine("#FLOOR 1 DESC EN:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 132175)
                                sw.WriteLine("#FLOOR 1 DESC KR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 197711)
                                sw.WriteLine("#FLOOR 1 DESC ES:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 263247)
                                sw.WriteLine("#FLOOR 1 DESC BR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1150)
                                sw.WriteLine("#FLOOR 1 DESC ??:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1203)
                            {
                                sw.WriteLine("#FLOOR 1 GOAL :\t\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                                sw.WriteLine();
                            }

                            //FLOOR 2                          
                            else if (mTrash[0] == 66739)
                                sw.WriteLine("#FLOOR 2 DESC EN:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 132275)
                                sw.WriteLine("#FLOOR 2 DESC KR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 197811)
                                sw.WriteLine("#FLOOR 2 DESC ES:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 263347)
                                sw.WriteLine("#FLOOR 2 DESC BR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1250)
                                sw.WriteLine("#FLOOR 2 DESC ??:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1303)
                            {
                                sw.WriteLine("#FLOOR 2 GOAL :\t\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                                sw.WriteLine();
                            }

                            //FLOOR 3
                            else if (mTrash[0] == 66839)
                                sw.WriteLine("#FLOOR 3 DESC EN:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 132375)
                                sw.WriteLine("#FLOOR 3 DESC KR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 197911)
                                sw.WriteLine("#FLOOR 3 DESC ES:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 263447)
                                sw.WriteLine("#FLOOR 3 DESC BR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1350)
                                sw.WriteLine("#FLOOR 3 DESC ??:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1403)
                            {
                                sw.WriteLine("#FLOOR 3 GOAL :\t\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                                sw.WriteLine();
                            }

                            //FLOOR 4
                            else if (mTrash[0] == 66939)
                                sw.WriteLine("#FLOOR 3 DESC EN:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 132475)
                                sw.WriteLine("#FLOOR 3 DESC KR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 198011)
                                sw.WriteLine("#FLOOR 3 DESC ES:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 263547)
                                sw.WriteLine("#FLOOR 3 DESC BR:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            else if (mTrash[0] == 1450)
                                sw.WriteLine("#FLOOR 3 DESC ??:\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));
                            //else if (mTrash[0] == 1403)
                            //{
                            //    sw.WriteLine("#FLOOR 3 GOAL :\t\t\t" + Encoding.ASCII.GetString(bByte).TrimEnd((Char)0));
                            //    sw.WriteLine();
                            //}
                            iLast = mTrash[1];


                        }
                        int iGoal = (int)fs.Length - (int)fs.Position- 4;
                        bByte = new byte[iGoal];
                        fs.Read(bByte, 0, iGoal);
                        sw.WriteLine("#GOAL :\t\t\t\t\t" + Encoding.UTF8.GetString(bByte).Trim((Char)0));

                    }



                    if (dDivisionInfo)
                    {
                        string[] sString = new string[] { "Perfect", "Great", "Good", "Bad", "Miss", "StepG", "StepW", "StepA", "StepB", "StepC" };
                        sw.WriteLine("[DIVISIONDATA]");
                        foreach (SplitData splitdata in nx20File.splitdata)
                        {
                            foreach (DivisionData divisionData in splitdata.divisionData)
                            {
                                foreach (DivisionInfo divisioninfo in divisionData.divisionInfo)
                                {
                                    if (divisioninfo.iScore == -1 || divisioninfo.iScore == 200)
                                        continue;
                                    sw.WriteLine("Split=" + (splitdata.iCurrentSplit + 1));
                                    sw.WriteLine("Block=" + (divisionData.iCurrentDivision + 1));
                                    if (divisioninfo.iScore > sString.Length)                                    
                                        sw.WriteLine("Score=" + divisioninfo.iScore.ToString());
                                    else
                                        sw.WriteLine("Score=" + sString[divisioninfo.iScore]);
                                    sw.WriteLine("Min=" + divisioninfo.iMin);
                                    sw.WriteLine("Max=" + divisioninfo.iMax);
                                    sw.WriteLine();
                                }
                                if (divisionData.divisionInfo.Count > 0)
                                    if (divisionData.divisionInfo[0].iScore != -1 && divisionData.divisionInfo[0].iScore != 200)
                                        sw.WriteLine("##################################################");
                            }
                        }
                    }                    
                }
            }



           





            fs.Close();

            return nx20File;
        }
    }
}
