using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nx20
{
    class NX10File
    {
        public List<SplitData> splitdata;
        public int iCol;
        public int iLevel;
    }

    class NX10Reader
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
                 lHeader[0][2] == 49 &&
                 lHeader[0][3] == 48))
                toReturn = true;


            fs.Close();

            return toReturn;
        }

        public NX10File Read(string sPath)
        {
            NX10File nx10File = new NX10File();

            FileStream fs = new FileStream(sPath, FileMode.Open);

            int[] iBytesHeader = new int[] {
                4,//firma
                4,//nada ?
                4,//cols                INT32 - Little Endian (DCBA)
                4,//splits ?
            //    4,//splits              INT32 - Little Endian (DCBA)
            //    8,//nada ?                
            };

            //int[] iBytesHeaderPrime = new int[] {
            //    4,//firma
            //    4,//nada ?
            //    4,//cols                INT32 - Little Endian (DCBA)
            //    4,//nada ?
            //    4,//basura?             INT32 - Little Endian (DCBA)
            //    //4,//splits              INT32 - Little Endian (DCBA)
            //    //8,//nada ?                
            //};

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
                4, //division info position
                1,//beatsplit
                1,//?
                1,//beat per measure
                1,//smooth                
                //4,//division INFO
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
            for (int i = 0; i < iBytesHeader.Length; i++)
            {
                bByte = new byte[iBytesHeader[i]];
                fs.Read(bByte, 0, iBytesHeader[i]);
                lHeader.Add(bByte);
            }

            if (!(lHeader[0][0] == 78 &&
                lHeader[0][1] == 88 &&
                lHeader[0][2] == 49 &&
                lHeader[0][3] == 48))
                return new NX10File();

            int iCol = BitConverter.ToInt32(lHeader[2], 0);         //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            nx10File.iCol = iCol;

            int iSplits = BitConverter.ToInt32(lHeader[3], 0);         //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            //nx10File.iCol = iCol;


            long iOldSplitPos = 0;
            long iOldDivisionPos = 0;
            long iOldNotePos = 0;
            long iOldDivisionInfoPos = 0;

            int iPos;


            List<SplitData> splitData = new List<SplitData>();
            for (int s = 0; s < iSplits; s++)
            {
                SplitData split = new SplitData();

                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                iOldSplitPos = fs.Position;

                //saltar a la posición nueva del split
                iPos = BitConverter.ToInt32(bByte, 0);
                fs.Position = iPos;



                byte[] bBlock = new byte[4];
                fs.Read(bBlock, 0, 4);
                int iDivision = BitConverter.ToInt32(bBlock, 0);    //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                split.divisionData = new List<DivisionData>();
                for (int d = 0; d < iDivision; d++)
                {
                    bByte = new byte[4];
                    fs.Read(bByte, 0, 4);
                    iOldDivisionPos = fs.Position;
                    iPos = BitConverter.ToInt32(bByte, 0);
                    fs.Position = iPos;

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
                    int iDivisionPosition = BitConverter.ToInt32(lStep[5], 0);
                    int iBeatSplit = (int)(lStep[6][0]);                    //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    int iBeatPerMeasure = (int)(lStep[8][0]);               //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    int iSmooth = (int)(lStep[9][0]);               //>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                    //int iDivisionInfo = 0;                //>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                    divisionData.divisionInfo = new List<DivisionInfo>();
                    if (iDivisionPosition != 0)
                    {
                        iOldDivisionInfoPos = fs.Position;
                        fs.Position = iDivisionPosition;
                        string[] sString = new string[] { "Perfect", "Great", "Good", "Bad", "Miss", "StepG", "StepW", "StepA", "StepB", "StepC" };
                       

                        int[] iMinValues = new int[10];
                        int[] iMaxValues = new int[10];
                        for (int di = 0; di < sString.Length; di++)
                        {
                            bByte = new byte[4];
                            fs.Read(bByte, 0, 4);
                            iMinValues[di] = BitConverter.ToInt16(bByte, 0);
                        }
                        for (int di = 0; di < sString.Length; di++)
                        {
                            bByte = new byte[4];
                            fs.Read(bByte, 0, 4);
                            iMaxValues[di] = BitConverter.ToInt16(bByte, 0);
                        }
                        for (int di = 0; di < sString.Length; di++)
                        {
                            DivisionInfo divisionInfo = new DivisionInfo();
                            divisionInfo.iScore = di;
                            divisionInfo.iMin = iMinValues[di];
                            divisionInfo.iMax = iMaxValues[di];
                            divisionData.divisionInfo.Add(divisionInfo);
                        }
                        fs.Position = iOldDivisionInfoPos;
                    }
                    
                    //for (int di = 0; di < iDivisionInfo; di++)
                    {
                    //    DivisionInfo divisionInfo = new DivisionInfo();
                    //    bByte = new byte[4];
                    //    fs.Read(bByte, 0, 4);
                    //    divisionInfo.iScore = BitConverter.ToInt32(bByte, 0);
                    //    bByte = new byte[2];
                    //    fs.Read(bByte, 0, 2);
                    //    divisionInfo.iMin = BitConverter.ToInt16(bByte, 0);
                    //    bByte = new byte[2];
                    //    fs.Read(bByte, 0, 2);
                    //    divisionInfo.iMax = BitConverter.ToInt16(bByte, 0);
                    //    divisionData.divisionInfo.Add(divisionInfo);
                    }

                    bByte = new byte[4];
                    fs.Read(bByte, 0, 4);
                    int iRows = BitConverter.ToInt32(bByte, 0);             //>>>>>>>>>>>>>>>>>>>>>>>>>>>>


                    divisionData.timming.fTotalOffset = fTotalOffset;
                    divisionData.timming.fBPM = fBPM;
                    divisionData.timming.fMystery = fMystery;
                    divisionData.timming.fOffset = fOffset;
                    divisionData.timming.fSpeed = fSpeed *-1;   //para emparejar con NX20
                    divisionData.timming.iBeatSplit = iBeatSplit;
                    divisionData.timming.iBeatPerMeasure = iBeatPerMeasure;
                    divisionData.timming.iSmooth = iSmooth;
                    divisionData.timming.iRows = iRows;


                    divisionData.step = new List<Step>();
                    for (int b = 0; b < iRows; b++)
                    {
                        
                        bByte = new byte[4];
                        fs.Read(bByte, 0, 4);

                        if (bByte[0] == 0 && bByte[1] == 0 && bByte[2] == 0 && bByte[3] == 0)
                        {
                            //converter
                            for (int c = 0; c < iCol; c++)
                            {
                                Step step = new Step();
                                step.iRow = b;
                                step.iCol = c;
                                divisionData.step.Add(step);
                            }
                        }
                        else
                        {
                            iPos = BitConverter.ToInt32(bByte, 0);
                            iOldNotePos = fs.Position;
                            fs.Position = iPos;


                            for (int c = 0; c < iCol; c++)
                            {
                                Step step = new Step();
                                step.iRow = b;
                                bByte = new byte[2];
                                fs.Read(bByte, 0, 2);
                                if (bByte[0] != 0 || bByte[1] != 0)   //por lo menos un tap
                                {
                                    step.iNote = bByte[0];
                                    step.iLayer = bByte[1];
                                    step.iCol = c;
                                }




                                //bByte = new byte[4];
                                //fs.Read(bByte, 0, 4);
                                //step.iCol = c;
                                //step.iNote = bByte[0];
                                //step.iLayer = bByte[1];
                                //step.iPlayer = bByte[2];
                                //step.iSpecial = bByte[3];

                                //converter
                                //if (step.iNote != 0)
                                divisionData.step.Add(step);
                            }
                            fs.Position = iOldNotePos;
                        }
                    }

                    divisionData.iCurrentDivision = d;
                    split.divisionData.Add(divisionData);

                    //fs.Position += 4;//nada
                    fs.Position = iOldDivisionPos;
                }

                fs.Position = iOldSplitPos;
                splitData.Add(split);
            }
            nx10File.splitdata = splitData;

            bool dDivisionInfo = false;
            //bool bTrash = false;
            foreach (SplitData splitdata in nx10File.splitdata)
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
            //if (lTrash.Count > 0)
            //    bTrash = true;


            sPath = Path.ChangeExtension(sPath, ".ini");
            if (File.Exists(sPath))
                File.Delete(sPath);

            if (dDivisionInfo)
            {
                using (StreamWriter sw = new StreamWriter(sPath, false))
                {
                    if (dDivisionInfo)
                    {
                        string[] sString = new string[] { "Perfect", "Great", "Good", "Bad", "Miss", "StepG", "StepW", "StepA", "StepB", "StepC" };
                        sw.WriteLine("[DIVISIONDATA]");
                        foreach (SplitData splitdata in nx10File.splitdata)
                        {
                            foreach (DivisionData divisionData in splitdata.divisionData)
                            {
                                foreach (DivisionInfo divisioninfo in divisionData.divisionInfo)
                                {
                                    if (divisioninfo.iMin == 0 && divisioninfo.iMax == 0)
                                        continue;
                                    sw.WriteLine("Split=" + (splitdata.iCurrentSplit + 1));
                                    sw.WriteLine("Block=" + (divisionData.iCurrentDivision + 1));
                                    sw.WriteLine("Score=" + sString[divisioninfo.iScore]);
                                    sw.WriteLine("Min=" + divisioninfo.iMin);
                                    sw.WriteLine("Max=" + divisioninfo.iMax);   
                                    
                                    sw.WriteLine();
                                }
                                if(divisionData.divisionInfo.Count > 0)
                                    sw.WriteLine("##################################################");
                            }
                           
                        }
                    }
                }
            }







            fs.Close();

            return nx10File;
        }
    }
}
