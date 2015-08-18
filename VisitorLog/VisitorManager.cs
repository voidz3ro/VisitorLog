using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Windows.Storage.FileProperties;
using VisitorLog;
using Windows.Data.Xml;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Windows;
using System.Threading.Tasks;
using VisitorLog.VisitorLogService;




namespace VisitorLog
{
    public class VisitorManager
    {
        public string firstname;
        public string lastname;
        public string companyrepresented;
        public string boeinghostname;
        public string badgenumber = "";
        public Boolean boeingEmp = false;
        public long bemsid;
        public string AdminInitialsOut;
        public string PreScreenedByInitials = "";
        public long EnteredByAdmin;
        public DateTime EnteredByAdminDate;
        public DateTime timein = DateTime.Now;
        public Boolean prerecorded = false;
        public int ID = 0;

        public bool AgreeToPrivacyStatement;

      // private VisitorLogServiceClient vlClient = new VisitorLogServiceClient();
        
        private VisitorLogServiceClient vlClient = new VisitorLogServiceClient(new VisitorLogServiceClient.EndpointConfiguration());

        public void setUser(string fname, string lname, string company, string hostname)
        {
            this.firstname = fname;
            this.lastname = lname;
            this.companyrepresented = company;
            this.boeinghostname = hostname;
        }

        public void setTimeIn(DateTime ntimein)
        {
            this.timein = ntimein;
        }


        public VisitorManager getUser()
        {
            return this;
        }


        public void parsePerson(XElement person)
        {
            this.firstname = person.Element("First-Name").Value;
            this.lastname = person.Element("Last-Name").Value;
            this.badgenumber = person.Element("Badge-Number").Value;
            this.boeingEmp = Convert.ToBoolean(person.Element("Boeing-Employee").Value);
            this.bemsid = Convert.ToInt64(person.Element("BEMSID").Value);
            this.boeinghostname = person.Element("Boeing-Host-Name").Value;
            this.PreScreenedByInitials = person.Element("Pre-Screened-by").Value;
            this.EnteredByAdmin = Convert.ToInt64(person.Element("EnteredByAdmin").Value);
            this.AgreeToPrivacyStatement = true; //always true why even track it.....
            this.companyrepresented = person.Element("Company-Represented").Value;
            this.timein = Convert.ToDateTime(person.Element("Time-In").Value);
            this.ID = Convert.ToInt32(person.Element("ID").Value);
        }


        Windows.Storage.StorageFolder sf;

        private async Task checkFileSystem()
        {
            await App.appman.checkFileSystem();
        }

        public void SetPrescreened(bool val)
        {
            if (val)
            {
                this.PreScreenedByInitials = App.appman.GetAdminInitials();
            }
            else
            {
                this.PreScreenedByInitials = "";
            }
           
        }


        public void SetAdminInitialsOut(bool val)
        {

            this.AdminInitialsOut = App.appman.GetAdminInitials();

        }

        public async Task update(string action)
        {
            await checkFileSystem();
            //Debug.WriteLine("Done checking file system");
            string prescreened = (this.PreScreenedByInitials != "") ? prescreened = "Yes" : prescreened = "No";
            bool ps = prescreened == "Yes" ? true : false;
            if (this.prerecorded == true) 
            { 
                this.EnteredByAdmin = 1;
                if (action == "Sign In")
                {
                    this.timein = DateTime.Now;
                }
            }

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            XDocument doc = XDocument.Load(file.Path);

            XElement newAction = new XElement("Action",
                            new XElement("Person",
                            new XElement("First-Name", this.firstname),
                            new XElement("Last-Name", this.lastname),
                            new XElement("Company-Represented", this.companyrepresented),
                            new XElement("Country", App.appman.GetCountry()),
                            new XElement("City", App.appman.GetCity()),
                            new XElement("Sublocation", App.appman.GetSublocation()),
                             new XElement("Location", ""),
                            new XElement("Entrance", ""),
                            new XElement("Date", DateTime.Now),
                            new XElement("Badge-Number", this.badgenumber),
                            new XElement("Boeing-Employee", this.boeingEmp),
                            new XElement("BEMSID", this.bemsid),
                            new XElement("Boeing-Host-Name", this.boeinghostname),
                            new XElement("Time-In", this.timein),
                            new XElement("Time-Out", ""),
                            new XElement("Admin-Initials-Out", this.AdminInitialsOut),
                            new XElement("Pre-Screened", prescreened),
                            new XElement("Pre-Screened-by", this.PreScreenedByInitials),
                            new XElement("EnteredByAdmin", this.EnteredByAdmin),
                            new XElement("Agree-To-Privacy-Statement", true),
                            new XElement("EnteredByAdminDate", this.EnteredByAdminDate),
                            new XElement("ID", this.ID)
                            ));

            newAction.SetAttributeValue("type", action);

            IEnumerable<XElement> people = doc.Root.Elements("Action").Elements("Person");

            foreach (XElement person in people)
            {
                if (Convert.ToInt32(person.Element("ID").Value) == this.ID)
                {                  
                    person.Parent.ReplaceWith(newAction);
                }
            }
            //Debug.WriteLine("updated node " + this.ID); 

            await FileIO.WriteTextAsync(file, doc.Root.ToString());

            //attempt sending record data to service.
            //try
            //{
            //    string recordResult = await vlClient.RecordActionAsync(action, this.companyrepresented, App.appman.GetCountry(), App.appman.GetCity(), App.appman.GetSublocation(), ps, App.appman.GetEntity());
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine("Error contacting service: " + e.Message);
            //}

        }

        public async void delete(int nid)
        {

            Debug.WriteLine("attempting removal...nid = " + nid);
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
            XDocument doc = XDocument.Load(file.Path);

            
            IEnumerable<XElement> people = doc.Root.Elements("Action").Elements("Person");

            foreach (XElement person in people)
            {
                if (Convert.ToInt32(person.Element("ID").Value) == nid)
                {
                    Debug.WriteLine("removeing node id " + nid);
                    person.Parent.Remove();
                }
            }

            await FileIO.WriteTextAsync(file, doc.Root.ToString());
        }

        public async Task record(string action) // record("Sign-in");
        {
            Debug.WriteLine("Recording " + action + ".");
            //some of the auto-logic
            //first check that the file exists and write it if not.
            await checkFileSystem();
            //Debug.WriteLine("Done checking file system");
            string prescreened = (this.PreScreenedByInitials != "") ? prescreened = "Yes" : prescreened = "No";
            if (this.prerecorded == true) { this.EnteredByAdmin = 1; }
            bool ps = prescreened == "yes" ? true : false;

            Debug.WriteLine("Getting file ");
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            Debug.WriteLine("file location is  " + file.Path);

           // Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");

            XDocument doc = XDocument.Load(file.Path);

            int newid = doc.Root.Elements().Count() + 1;

             DateTime ntimein;
           //  Debug.WriteLine("prerecorded " + this.prerecorded + ".");
             if (this.prerecorded == true) { ntimein = this.timein; } else { ntimein = DateTime.Now; }
             //Debug.WriteLine("ntimein " + ntimein + ".");
            XElement newAction = new XElement("Action",
            new XElement("Person",
            new XElement("First-Name", this.firstname),
            new XElement("Last-Name", this.lastname),
            new XElement("Company-Represented", this.companyrepresented),
            new XElement("Country", App.appman.GetCountry()),
            new XElement("City", App.appman.GetCity()),
            new XElement("Sublocation", App.appman.GetSublocation()),
            new XElement("Location", ""),
            new XElement("Entrance", ""),
            new XElement("Date", DateTime.Now),
            new XElement("Badge-Number", this.badgenumber),
            new XElement("Boeing-Employee", this.boeingEmp),
            new XElement("BEMSID", this.bemsid),
            new XElement("Boeing-Host-Name", this.boeinghostname),
            new XElement("Time-In", ntimein),
            new XElement("Time-Out", ""),
            new XElement("Admin-Initials-Out", this.AdminInitialsOut),
            new XElement("Pre-Screened", prescreened),
            new XElement("Pre-Screened-by", this.PreScreenedByInitials),
            new XElement("EnteredByAdmin", this.EnteredByAdmin),
            new XElement("Agree-To-Privacy-Statement", true),
            new XElement("EnteredByAdminDate", this.EnteredByAdminDate),
            new XElement("ID", newid)
            ));

            newAction.SetAttributeValue("type", action);
            doc.Root.Add(newAction);

            //Debug.WriteLine("Attempting to write xml to file");
            //Debug.WriteLine(newAction);
            this.ID = newid;
            
            await FileIO.WriteTextAsync(file, doc.Root.ToString());

            //try
            //{
            //    string recordResult = await vlClient.RecordActionAsync(action, this.companyrepresented, App.appman.GetCountry(), App.appman.GetCity(), App.appman.GetSublocation(), ps, App.appman.GetEntity());
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine("Error contacting service: " + e.Message);
            //}
        }
    }


}
