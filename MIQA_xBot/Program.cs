using EvilDICOM.Network;
using EvilDICOM.Network.DIMSE.IOD;
using EvilDICOM.Network.Querying;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

/// <summary>
/// MIQA_xBot is a single purpose implementation of the generalized xBot(https://github.com/SUSradfys/xBot) 
/// MIQA_xBot sends instructions to a ARIA Database Daemon service.
/// The information is used to conduct a DICOM CMOVE operation from the ARIA OIS to MIQA .
/// The EvilDICOM (https://github.com/rexcardan/Evil-DICOM) library is used as a middle hand.
/// </summary>

namespace MIQA_xBot
{
    class Program
    {
        // sender end
        static string AEtitleDB = "ARIADB";
        static int portDB = 57347;

        // reciever end
        static string ip = "147.220.155.249";
        static string AEtitle = "MIQA_PACS";
        static int port = 104;

        // mail part
        static string recipient = "dicomdaemon@skane.se";
        static string subject = "MIQA export";

        [STAThread]
        static void Main(string[] args)
        {
            List<CFindImageIOD> iods = new List<CFindImageIOD>();
            Entity daemon = Entity.CreateLocal(AEtitleDB, portDB);

            // define the local service class
            var me = Entity.CreateLocal("EvilDICOMC", 50400);
            var scu = new DICOMSCU(me);

            // define the query builder
            var qb = new QueryBuilder(scu, daemon);
            ushort msgId = 1;

            // define the recievr
            Entity reciever = new Entity(AEtitle, ip, port);
            DICOMSCP scp = new DICOMSCP(reciever);

            // read the xml file
            string xml = File.ReadAllText(Settings.xPorterPath);
            Xports xPort = xml.ParseXML<Xports>();

            // Query plan
            DataTable plans = new DataTable();
            if (!String.IsNullOrEmpty(xPort.Xporter.SQLstring))
            {
                SqlInterface.Connect();
                plans = xPort.Query();
                SqlInterface.Disconnect();
            }

            // loop through plans
            foreach (DataRow row in plans.Rows)
            {
                var patId = (string)row["PatientId"];
                var planUID = (string)row["UID"];
                iods.Add(new CFindImageIOD() { PatientId = patId, SOPInstanceUID = planUID });
                // loop through items and query based on type
                foreach (string item in xPort.Xporter.include.ToList())
                {
                    string itemSqlString = string.Empty;
                    switch (item)
                    {
                        case "planDose":
                            itemSqlString = "select distinct UID = DoseMatrix.DoseUID from DoseMatrix where DoseMatrix.PlanSetupSer = " + (Int64)row["PlanSer"];
                            break;
                        case "fieldDoses":
                            itemSqlString = "select distinct UID = DoseMatrix.DoseUID from DoseMatrix, Radiation where Radiation.PlanSetupSer = " + (Int64)row["PlanSer"] + " and DoseMatrix.RadiationSer = Radiation.RadiationSer";
                            break;
                        case "slices":
                            itemSqlString = "select UID=Slice.SliceUID from Slice, Image, StructureSet, PlanSetup where PlanSetup.PlanSetupSer=" + (Int64)row["PlanSer"] + " and StructureSet.StructureSetSer=PlanSetup.StructureSetSer and Image.ImageSer=StructureSet.ImageSer and Slice.SeriesSer=Image.SeriesSer";
                            break;
                        case "structures":
                            itemSqlString = "select UID=StructureSet.StructureSetUID from PlanSetup, StructureSet where PlanSetup.PlanSetupSer=" + (Int64)row["PlanSer"] + " and StructureSet.StructureSetSer=PlanSetup.StructureSetSer";
                            break;
                        case "images":
                            itemSqlString = "select UID=Slice.SliceUID from Slice, ImageSlice, Radiation where Radiation.PlanSetupSer = " + (Int64)row["PlanSer"] + " and ImageSlice.ImageSer = Radiation.RefImageSer and Slice.SliceSer = ImageSlice.SliceSer";
                            break;
                        case "records":
                            itemSqlString = "select UID=TreatmentRecord.TreatmentRecordUID from TreatmentRecord, RTPlan where RTPlan.PlanSetupSer= " + (Int64)row["PlanSer"] + " and TreatmentRecord.RTPlanSer=RTPlan.RTPlanSer";
                            break;
                        default:
                            itemSqlString = String.Empty;
                            break;
                    }
                    if (!String.IsNullOrEmpty(itemSqlString))
                    {
                        DataTable includeItem = SqlInterface.Query(itemSqlString);
                        foreach (DataRow itemRow in includeItem.Rows)
                        {
                            iods.Add(new CFindImageIOD() { PatientId = patId, SOPInstanceUID = (string)itemRow["UID"] });
                        }
                    }

                }
            }

            // overwrite lastActivity
            if (plans.Rows.Count > 0)
            {
                DateTime lastPlan = (DateTime)plans.Rows[plans.Rows.Count - 1]["MAXDate"];

                // write xml
                using (FileStream fs = new FileStream(Settings.xPorterPath, FileMode.Create))
                {
                    XmlSerializer _xSer = new XmlSerializer(typeof(Xports));

                    _xSer.Serialize(fs, xPort);
                }

            }

            if (xPort.Xporter.active)
            {
                Console.WriteLine(iods.Count.ToString());
                // Remove duplicate UIDs
                if (!xPort.Xporter.allowDoublets)
                    iods = ListHandler.Unique(iods);

                Console.WriteLine(iods.Count.ToString());
                foreach (var iod in iods)
                {
                    // Send it
                    //scu.SendCMoveImage(daemon, iod, xPort.Xporter.AEtitle, ref msgId);
                    // Add logging
                }
            }
        }
    }
}
