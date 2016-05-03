using System;

namespace EgcsCommon
{
    public class FiscalYear
    {
        DateTime startDate;
        DateTime endDate;
        int[] _fiscalYears;


        public FiscalYear(DateTime start, DateTime end)
        {
            this.startDate = start;
            this.endDate = end;
        }

        public string hi()
        {
            return "hi";
        }

        public int[] getFiscalYears()
        {
            if (this.startDate != null && this.endDate != null)
            {
                // this is somethign we can extract in a seperate class called fiscalyears or something
                int startMonth = this.startDate.Month;
                int endMonth = this.endDate.Month;
                int startYear = this.startDate.Year;
                int endYear = this.endDate.Year;
                int numberOfYears = (endYear - startYear);

                if (startMonth < 4)
                {
                    if (endMonth > 4)
                    {
                        numberOfYears = numberOfYears + 2;
                        endYear = endYear + 1;
                    }
                    else
                    {
                        numberOfYears = numberOfYears + 1;
                    }
                }
                else
                {
                    startYear = startYear + 1;
                    if (endMonth > 4)
                    {
                        numberOfYears = numberOfYears + 1;
                        endYear = endYear + 1;
                    }
                }

                _fiscalYears = new int[numberOfYears];

                for (var i = 0; i < numberOfYears; i++)
                {
                    _fiscalYears[i] = startYear + i;
                }

            }
            return _fiscalYears;
        }



    }
}


