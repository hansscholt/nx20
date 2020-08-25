using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nx20
{
   
    class STXFile
    {
        public List<SplitData> splitdata;
        public string sSongTitle;
        public string sSongArtist;
        public string sStepAuthor;
        public int iCol;
        public int iLevel;
        public int iTotalStep;
        public string sFile;
    }
    class STXReader
    {
        int iDecPos;

        public List<STXFile> Read(string sPath)
        {
            List<STXFile> lStxFile = new List<STXFile>();


            iDecPos = 0;
            

            FileStream fs = new FileStream(sPath, FileMode.Open);
            fs.Position = 60;

            string sSongTitle;
            byte[] bByte = new byte[64];
            fs.Read(bByte, 0, 64);
            sSongTitle = System.Text.Encoding.Default.GetString(bByte);

            string sSongArtist;
            bByte = new byte[64];
            fs.Read(bByte, 0, 64);
            sSongArtist = System.Text.Encoding.Default.GetString(bByte);

            string sStepAuthor;
            bByte = new byte[64];
            fs.Read(bByte, 0, 64);
            sStepAuthor = System.Text.Encoding.Default.GetString(bByte);

            //practice
            //normal
            //hard
            //nightmare
            //crazy
            //full
            //half
            //division
            //lightmap
            int[] iCol = new int[] { 5, 5, 5, 10, 5, 10, 6, 5, 3 };

            string[] sFile = new string[] { "PR.NX", "NO.NX", "HD.NX", "NM.NX", "CR.NX", "FR.NX", "HF.NX", "DV.NX", "LM.NX" };


            int[] iStepPosition = new int[9];
            for (int i = 0; i < 9; i++)
            {
                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                iStepPosition[i] = BitConverter.ToInt16(bByte, 0);
            }


            for (int i = 0; i < 9; i++)
            {
                int iTotalStep = 0;
                STXFile stxFile = new STXFile();

                fs.Position = iStepPosition[i];
                bByte = new byte[4];
                fs.Read(bByte, 0, 4);
                stxFile.iLevel = BitConverter.ToInt16(bByte, 0);
                stxFile.sSongTitle = sSongTitle;
                stxFile.sSongArtist = sSongArtist;
                stxFile.sStepAuthor = sStepAuthor;

                stxFile.iCol = iCol[i];
                stxFile.sFile = sFile[i];

                int iSplits = 0;
                List<int> lBlockCount = new List<int>();
                //50 split máximo
                for (int s = 0; s < 50; s++)
                {
                    int iSplit;
                    bByte = new byte[4];
                    fs.Read(bByte, 0, 4);
                    iSplit = BitConverter.ToInt16(bByte, 0);

                    //sumo la cantidad de splits y guardo la cantidad de bloques por split
                    if (iSplit > 0)
                    {
                        iSplits ++;
                        lBlockCount.Add(iSplit);
                    }
                }

                List<SplitData> splitData = new List<SplitData>();
                for (int s = 0; s < iSplits; s++)
                {                    
                    int iBlock = lBlockCount[s];

                    SplitData split = new SplitData();
                    split.divisionData = new List<DivisionData>();
                    for (int b = 0; b < iBlock; b++)
                    {
                        iDecPos = 0;
                        int iStepSize;
                        bByte = new byte[4];
                        fs.Read(bByte, 0, 4);
                        iStepSize = BitConverter.ToInt16(bByte, 0);

                        bByte = new byte[iStepSize];
                        fs.Read(bByte, 0, iStepSize);


                        byte[] dec;
                        using (GZipStream stream = new GZipStream(new MemoryStream(bByte), CompressionMode.Decompress))
                        {
                            const int size = 4096;
                            byte[] buffer = new byte[size];
                            using (MemoryStream memory = new MemoryStream())
                            {
                                int c = 0;
                                do
                                {
                                    c = stream.Read(buffer, 0, size);
                                    if (c > 0)
                                        memory.Write(buffer, 0, c);
                                }
                                while (c > 0);
                                dec = memory.ToArray();
                            }
                        }

                        //int iDecPos = 0;
                        float fBPM = BitConverter.ToSingle(decRead(dec, iDecPos, 4), 0);
                        int iBeatPerMeasure = BitConverter.ToInt32(decRead(dec, iDecPos, 4), 0);
                        int iBeatSplit = BitConverter.ToInt32(decRead(dec, iDecPos, 4), 0);
                        float fOffset = BitConverter.ToInt32(decRead(dec, iDecPos, 4), 0);
                        //que pasa con esos 80 bytes?
                        iDecPos = 96;
                        float fSpeed = BitConverter.ToInt32(decRead(dec, iDecPos, 4), 0);
                        fSpeed /= 1000.0f;
                        iDecPos = 128;
                        int iRows = BitConverter.ToInt32(decRead(dec, iDecPos, 4), 0);

                        iDecPos = 132;

                        DivisionData divisionData = new DivisionData();
                        divisionData.timming.fBPM = fBPM;
                        divisionData.timming.fOffset = fOffset * 10.0f;
                        divisionData.timming.fSpeed = fSpeed*-1;
                        divisionData.timming.iBeatSplit = iBeatSplit;
                        divisionData.timming.iBeatPerMeasure = iBeatPerMeasure;
                        divisionData.timming.iRows = iRows;
                        divisionData.timming.fMystery = iRows;


                        divisionData.step = new List<Step>();

                        for (int r = 0; r < iRows; r++)
                        {
                            //5 para single
                            //10 para double
                            //3 para lightmap
                            for (int c = 0; c < 13; c++)
                            {
                                int iStep = (int)decRead(dec, iDecPos, 1)[0];

                                if (c >= iCol[i])
                                    continue;
                                Step step = new Step();
                                if (iStep != 0)
                                {
                                    iTotalStep++;
                                    
                                    step.iRow = r;
                                    step.iCol = c;
                                    step.iNote = iStep;
                                    divisionData.step.Add(step);
                                }
                                else
                                {
                                    step.iCol = c;
                                    divisionData.step.Add(step);
                                }

                            }
                        }

                        divisionData.iCurrentDivision = b;
                        split.divisionData.Add(divisionData);

                        split.iCurrentSplit = s;
                    }
                    splitData.Add(split);
                }
                stxFile.splitdata = splitData;

                stxFile.iTotalStep = iTotalStep;
                lStxFile.Add(stxFile);
            }

            


            return lStxFile;
        }

        byte[] decRead(byte[] bDec, int iPos, int iLength)
        {
            byte[] b = new byte[iLength];

            for (int l = 0; l < iLength; l++)            
                b[l] = bDec[iPos + l];

            iDecPos += iLength;
            return b;
        }
    }
}
