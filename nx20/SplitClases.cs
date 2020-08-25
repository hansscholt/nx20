using System;
using System.Collections.Generic;
using System.Text;

namespace nx20
{
    public struct Timming
    {
        public float fTotalOffset;
        public float fBPM;
        public float fMystery;
        public float fOffset;
        public float fSpeed;
        public int iBeatSplit;
        public int iBeatPerMeasure;
        public int iSmooth;
        public int iRows;
    }
    public struct Step
    {
        public int iRow;
        public int iCol;
        public int iPlayer;
        public int iLayer;
        public int iNote;
        public int iSpecial;
    }
    public struct DivisionInfo
    {
        public int iScore;
        public int iMin;
        public int iMax;
    }
    public class DivisionData
    {
        public float fSpace;
        //public int iBeatSplit;
        public int iCurrentDivision;
        public List<DivisionInfo> divisionInfo;
        public Timming timming;
        public List<Step> step;
    }

    public class SplitData
    {
        //public int iLevel;
        //public int iCol;
        public int iCurrentSplit;
        public float fHeight;
        public List<DivisionData> divisionData;
    }


}
