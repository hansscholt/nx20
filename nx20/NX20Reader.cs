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
            List<int[]> lTrash = new List<int[]>();
            for (int t = 0; t < iTrash; t++)
            {
                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                int iCode = BitConverter.ToInt32(bByte, 0);
                fs.Read(bByte, 0, 4);
                int iData = BitConverter.ToInt32(bByte, 0);

                if (iCode == 1001)
                    iLevel = iData;
                
                lTrash.Add(new int[] { iCode, iData });
            }
            nx20File.iLevel = iLevel;

            bByte = new byte[4];
            fs.Read(bByte, 0, 4);
            int iSplit = BitConverter.ToInt32(bByte, 0);       //>>>>>>>>>>>>>>>>>>>>>>>>>>>>

            fs.Position += 8;//nada
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
                    //split.iCol = iCol;
                    //split.iLevel = iLevel;

                    //split.fSpace = 48.0f / split.divisionData[d].timming.iBeatSplit;
                    //Console.WriteLine(split.fSpace);
                    split.iCurrentSplit = s;
                    //split.iBeatSplit = split.divisionData[d].timming.iBeatSplit;
                }
                splitData.Add(split);
                fs.Position += 8;
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
                        dDivisionInfo = true;
                        break;
                    }
                }
            }
            if (lTrash.Count > 0)            
                bTrash = true;


            sPath = Path.ChangeExtension(sPath, ".ini");
            if (File.Exists(sPath))
                File.Delete(sPath);

            if (dDivisionInfo || bTrash)
            {
                using (StreamWriter sw = new StreamWriter(sPath, false))
                {
                    if (bTrash)
                    {
                        if (lTrash.Count > 0)
                        {
                            sw.WriteLine("[METADATA]");
                            foreach (int[] trash in lTrash)
                            {
                                sw.WriteLine(trash[0] + "=" + trash[1]);
                            }
                            sw.WriteLine();
                        }
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
