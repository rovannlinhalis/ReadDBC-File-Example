using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadDBC_File_Example
{
    class Program
    {
        /*
         
BO_ 199 WheelInfoIEEE: 8 ABS
 SG_ WheelSpeedFR : 32|32@1- (1,0) [0|1300] "1/min"  GearBox,EngineControl
 SG_ WheelSpeedFL : 0|32@1- (1,0) [0|1300] "1/min"  GearBox,EngineControl

BO_ 200 WheelInfo: 8 ABS
 SG_ WheelSpeedRR : 48|16@1+ (0.02,0) [0|1300] "1/min"  GearBox
 SG_ WheelSpeedRL : 32|16@1+ (0.02,0) [0|1300] "1/min"  GearBox
 SG_ WheelSpeedFR : 16|16@1+ (0.02,0) [0|1300] "1/min"  GearBox
 SG_ WheelSpeedFL : 0|16@1+ (0.02,0) [0|1300] "1/min"  GearBox

BO_ 1020 GearBoxInfo: 1 GearBox
 SG_ EcoMode : 6|2@1+ (1,0) [0|1] ""  DashBoard,EngineControl
 SG_ ShiftRequest : 3|1@1+ (1,0) [0|0] ""  EngineControl
 SG_ Gear : 0|3@1+ (1,0) [1|5] ""  ABS,DashBoard,EngineControl 
             
             */
        static void Main(string[] args)
        {
            string fileContent =
@"
BO_ 199 WheelInfoIEEE: 8 ABS
 SG_ WheelSpeedFR : 32|32@1- (1,0) [0|1300] ""1/min""  GearBox,EngineControl
 SG_ WheelSpeedFL : 0|32@1- (1,0) [0|1300] ""1/min""  GearBox,EngineControl

BO_ 200 WheelInfo: 8 ABS
 SG_ WheelSpeedRR : 48|16@1+ (0.02,0) [0|1300] ""1/min""  GearBox
 SG_ WheelSpeedRL : 32|16@1+ (0.02,0) [0|1300] ""1/min""  GearBox
 SG_ WheelSpeedFR : 16|16@1+ (0.02,0) [0|1300] ""1/min""  GearBox
 SG_ WheelSpeedFL : 0|16@1+ (0.02,0) [0|1300] ""1/min""  GearBox

BO_ 1020 GearBoxInfo: 1 GearBox
 SG_ EcoMode : 6|2@1+ (1,0) [0|1] """"  DashBoard,EngineControl
 SG_ ShiftRequest : 3|1@1+ (1,0) [0|0] """"  EngineControl
 SG_ Gear : 0|3@1+ (1,0) [1|5] """"  ABS,DashBoard,EngineControl ";



            DBCFile file = new DBCFile(fileContent);
            file.Print();


            Console.WriteLine("-------------------------------------------");

            //From this I need to get 2 rows in DB:
            //199  WheelInfoIEEE 8  WheelSpeedFR   32   32
            //199  WheelInfoIEEE 8  WheelSpeedFL    0   32

            DBCHeader h = file.Headers.FirstOrDefault();
            foreach (DBCRecord r in h.Records)
            {
                Console.WriteLine((int)h.HeaderType + " " + h.HeaderType + " " + h.NameID + " - " + r.Name + " - " + r.Value1A + " / " + r.Value1B) ;
            }
        }
    }

    public class DBCFile
    {
        public DBCFile(string content = null)
        {
            if (!string.IsNullOrEmpty(content))
            {
                string[] lines = content.Replace("\r","").Split('\n');

                foreach (string line in lines)
                {
                    if (!String.IsNullOrWhiteSpace(line.Trim()))
                    {
                        if (line.Trim().StartsWith(DBCHeader.Prefix))
                        {
                            //it's a header
                            Headers.Add(new DBCHeader(line));
                        }
                        else if (line.Trim().StartsWith(DBCRecord.Prefix))
                        {
                            //it's a record
                            Headers.LastOrDefault().Records.Add(new DBCRecord(line));
                        }
                        else
                        {
                            //unidentified line
                        }
                    }
                }
            }
        }


        public List<DBCHeader> Headers { get; set; } = new List<DBCHeader>();


        public void Print()
        {
            foreach (DBCHeader h in Headers)
            {
                Console.WriteLine(h.HeaderType.ToString() + " - " + h.Name);

                foreach (DBCRecord r in h.Records)
                {
                    Console.WriteLine($"{r.Name} - {r.Value1} - {r.Value2} - {r.Value3} - {r.Value4} - {r.Value5} ");
                }
            }
        }
    }
    public class DBCHeader
    {
        public static string Prefix = "BO_";
        public DBCHeader(string line = null)
        {
            if (!String.IsNullOrWhiteSpace(line) && line.Contains(":"))
            {
                //BO_ 199 WheelInfoIEEE: 8 ABS
                string[] vs = line.Split(':');
                this.Name = vs.Length >1 ?  vs[1].Trim() : String.Empty;

                string[] haux = vs[0].Split(' ');
                if (int.TryParse(haux[1], out int id))
                {
                    this.HeaderType = (DBCHeaderType)id;
                }
            }
        }
        public DBCHeaderType HeaderType { get; set; }
        public string Name { get; set; }
        public string NameID { get => Name.Split(' ')[0]; }

        public List<DBCRecord> Records { get; set; } = new List<DBCRecord>();
    }
    public class DBCRecord
    {
        public static string Prefix = "SG_";
        public DBCRecord(string line = null)
        {
            if (!String.IsNullOrWhiteSpace(line) && line.Contains(":"))
            {
                //SG_ WheelSpeedRR : 48|16@1+ (0.02,0) [0|1300] "1/min"  GearBox
                string[] aux  = line.Trim().Split(':');
                string[] values = aux[1].Trim().Split(' ');
                this.Name = aux[0].Trim().Split(' ')[1];
                this.Value1 = values.Length > 0 ? values[0] : String.Empty;
                this.Value2 = values.Length > 1 ? values[1] : String.Empty;
                this.Value3 = values.Length > 2 ? values[2] : String.Empty;
                this.Value4 = values.Length > 3 ? values[3] : String.Empty;
                this.Value5 = values.Length > 4 ? values[4] : String.Empty;
            }
        }

        public string Name { get; set; }
        public string Value1 { get; set; }
        public string Value1A { get => Value1.Split('|')[0]; }
        public string Value1B { get => Value1.Split('|')[1].Split('@')[0]; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
        public string Value5 { get; set; }

    }
    public enum DBCHeaderType
    {
        Undefined = -1,
        WheelInfoIEEE = 199,
        WheelInfo = 200,
        GearBoxInfo = 1020
    }
}
