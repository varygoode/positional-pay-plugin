using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
	[Info("Positional Pay", "varygoode", "1.0.0")]
	[Description("Pay players for positional work")]

	internal class PositionalPay : CovalencePlugin
	{
		#region Fields

		private const string PermAdmin = "positionalpay.admin";
		private const string PermManage = "positionalpay.manage";
		private const string PermUse = "positionalpay.use";

		[PluginReference]
        private Plugin StateIdentification;

        [PluginReference]
        private Plugin Banking;

        private StoredData storedData;
        private Configuration config;

		#endregion Fields

		#region Init

        private void Init()
        {
            permission.RegisterPermission(PermAdmin, this);
            permission.RegisterPermission(PermManage, this);
            permission.RegisterPermission(PermUse, this);

            storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);

            var lastJob = storedData.Jobs.Values.Select(l => l.Last()).OrderByDescending(t => t.ID).FirstOrDefault();
            Job.CurrentID = lastJob != null ? lastJob.ID : 0;

            var lastPosition = storedData.Positions.Values.Select(l => l.Last()).OrderByDescending(t => t.ID).FirstOrDefault();
            Position.CurrentID = lastPosition != null ? lastPosition.ID : 0;
        }

        #endregion Init

        #region Hooks

        private void Loaded()
        {
            if (Economics == null)
            {
                Puts("Economics not found! Find the plugin here: https://umod.org/plugins/economics");
            }

            if (Banking == null)
            {
            	Puts("Banking not found!");
            }
        }

        private void OnServerSave() => SaveData();

        private void Unload() => SaveData();

        #endregion Hooks

        #region Localization

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoUse"] = "You are not permitted to use that command.",
                ["NoJobs"] = "No jobs have been created.",
                ["NoPositions"] = "You currently have no positions.",
                ["Separator"] = "-----------------------------",

                ["PosInfo0"] = "A position consists of a filled flag, position ID, job title, which job title they report to, which "
                             + "job titles report to them, current clock-in/clock-out times, and current accumulated paycheck.",
                ["PosInfo1"] = "Use '/position list' to view your positions.",
                ["PosInfo2"] = "Use '/position create' to create a new position.\nUse '/position remove PosID#' to remove position with ID number PosID#.",
                ["PosInfo3"] = "Use '/position clockin PosID#' to Clock-In for your position with ID number PosID#.\n"
                             + "Use '/position clockout PosID#' to Clock-Out from your position with ID number PosID#.",
                ["PosInfo4"] = "Use '/position getpaycheck' to receive pay in coin that has accumulated in your paycheck for all of your positions.",
                ["PosInfo5"] = "Use '/position quit PosID#' to quit your position with ID number PosID#. You forfeit any unclaimed pay.",
                ["PosInfo6"] = "Use '/position hire PosID# Name' to hire Name for the position with ID number PosID#.\n"
                             + "Use '/position fire PosID# Name' to fire Name from the position with ID number PosID#.",
                ["PosInfo7"] = "Use '/position edit PosID# FieldName NewValue' to edit the field FieldName of the position with ID PosID# to NewValue.\n"
                             + "Field Names: JOBTITLE, REPORT_TO, REPORTS_ADD, REPORTS_REMOVE",

                ["PosListHeader"] = "<color=#00D8D8>YOUR POSITIONS</color>",
                ["ClockedIN"] = "<color=#66FF00>CLOCKED-IN</color>",
                ["ClockedOUT"] = "<color=#FF0000>CLOCKED-OUT</color>",

                ["JobInfo0"] = "A job title consists of a name, job title ID, description, daily (OOC hourly) payrate, and an ability group.",
                ["JobInfo1"] = "Use '/job list' to view jobs.",
                ["JobInfo2"] = "Use '/job create Name \"Description\" Payrate# AbilityGroup' to create a job called Name, " 
                             + "described by Description, paid Payrate# hourly, and abilities from AbilityGroup.",
                ["JobInfo3"] = "Use '/job delete JobID#' to permanently remove the job with ID number JobID#.",
                ["JobInfo4"] = "Use '/job edit JobID# FieldName NewValue' to edit the field FieldName of the job with ID number PosID# to NewValue.\n"
                             + "Field Names: NAME, DESCRIPTION, PAYRATE, GROUP",
                ["JobCreateSuccess"] = "You have successfully created the {0} job with id #{1}",
                ["JobDeleteNotFound"] = "Cannot find a job with id #{0} to delete",
                ["JobDeleteSuccess"] = "You have successfully deleted the {0} job with id #{1}"
            }, this);
        }

        #endregion Localization

        #region Commands

        [Command("position", "pos")]
        private void CommandPosition(IPlayer iPlayer, string command, string[] args)
        {
        	if (!iPlayer.HasPermission(PermAdmin) && !iPlayer.HasPermission(PermManage))
        	{
        		return;
        	}

        	if (args.Length < 1)
        	{
        		var message = "Usage: /position info";
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermUse)) message += "|list|clockin|clockout|getpaycheck|quit";
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermManage)) message += "|create|remove|hire|fire|edit";

                iPlayer.Message(message);
                return;
        	}

        	switch (args[0].ToLower())
        	{
        		case "info":
        		    iPlayer.Reply(Lang("PosInfo0", iPlayer.Id, command));
        		    iPlayer.Reply(Lang("PosInfo1", iPlayer.Id, command));
                    iPlayer.Reply(Lang("PosInfo2", iPlayer.Id, command));
                    iPlayer.Reply(Lang("PosInfo3", iPlayer.Id, command));
                    iPlayer.Reply(Lang("PosInfo4", iPlayer.Id, command));
                    iPlayer.Reply(Lang("PosInfo5", iPlayer.Id, command));
                    iPlayer.Reply(Lang("PosInfo6", iPlayer.Id, command));
                    iPlayer.Reply(Lang("PosInfo7", iPlayer.Id, command));

        		    return;

        		case "list":

        		    return;

        		case "clockin":

        		    return;

        		case "clockout":

        		    return;

        		case "getpaycheck":

        		    return;

        		case "quit":

        		    return;

        		case "create":

        		    return;

        		case "remove":

        		    return;

        		case "hire":

        		    return;

        		case "fire":

        		    return;

        		case "payrate":

        		    return;
        	}
        }

        [Command("job")]
        private void CommandJob(IPlayer iPlayer, string command, string[] args)
        {
        	if (!iPlayer.HasPermission(PermAdmin) && !iPlayer.HasPermission(PermManage))
        	{
        		return;
        	}

        	if (args.Length < 1)
        	{
        		var message = "Usage: /job info";
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermUse)) message += "|list";
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermManage)) message += "|create|remove|edit";

                iPlayer.Message(message);
                return;
        	}

        	switch (args[0].ToLower())
        	{
        		case "info":
        		    iPlayer.Reply(Lang("JobInfo0", iPlayer.Id, command));
        		    iPlayer.Reply(Lang("JobInfo1", iPlayer.Id, command));
                    iPlayer.Reply(Lang("JobInfo2", iPlayer.Id, command));
                    iPlayer.Reply(Lang("JobInfo3", iPlayer.Id, command));
                    iPlayer.Reply(Lang("JobInfo4", iPlayer.Id, command));

        		    return;

        		case "list":
                    List<Job> allJobs = new List<Job>();

                    for(int i = 0; i < Job.CurrentID; job++)
                    {
                        Job currentJob = FindJobWithID(i);
                        if(currentJob != null)
                        {
                            allJobs.Add(currentJob);
                        }
                        
                    }

                    if(allJobs.IsEmpty())
                    {
                        iPlayer.Reply(Lang("NoJobs", iPlayer.Id, command));
                        return;
                    }

                    string jobsOutput = "";
                    foreach (var job in allJobs)
                    {
                        jobsOutput += job.Title + " (ID # " + job.ID + ")\n";
                    }

                    iPlayer.Reply(jobsOutput);

        		    return;

        		case "create":
                    if(args.Length < 5)
                    {
                        iPlayer.Reply(Lang("JobInfo2", iPlayer.Id, command));
                        return;
                    }

                    Job newJob = Job(args[1], args[2], args[3], args[4]);

                    storedData.Jobs.Add(newJob.ID, new List<Job>() { newJob });

                    iPlayer.Reply(Lang("JobCreateSuccess", iPlayer.Id, newJob.Title, newJob.ID));

        		    return;

        		case "delete":
                    if(args.Length < 2)
                    {
                        iPlayer.Reply(Lang("JobInfo3", iPlayer.Id, command));
                        return;
                    }

                    Job deleteJob = FindJobWithID(args[1]);

                    if(deleteJob == null)
                    {
                        iPlayer.Reply(Lang("JobDeleteNotFound", iPlayer.Id, args[1]));
                        return;
                    }

                    iPlayer.Reply(Lang("JobDeleteSuccess", iPlayer.Id, deleteJob.Title, deleteJob.ID));
                    storedData.Remove(deleteJob.ID);

        		    return;

        		case "edit":

        		    return;
        	}
        }

        #endregion Commands

        #region Methods

        private bool EditJob(Job job, JobEditField field, string value)
        {
            switch(field)
            {
                case JobEditField.NAME:
                    job.Name = value.ToUpper();
                    return true;

                case JobEditField.DESCRIPTION:
                    job.Description = value;
                    return true;

                case JobEditField.PAYRATE:
                    double newRate;
                    if(!Double.TryParse(value, out newRate)) return false;
                    job.PayRate = newRate;
                    return true;

                case JobEditField.GROUP:
                    job.AbilityGroup = value;
                    return true;
            }
            return false;
        }

        #endregion Methods

        #region API

        #endregion API

        #region Helpers

        private IPlayer GetActivePlayerByUserID(string userID)
        {
            foreach (var player in players.Connected)
                if (player.Id == userID) return player;
            return null;
        }

        public BasePlayer GetAnyPlayerByUserID(string userID)
        {
            foreach (var player in BasePlayer.allPlayerList)
                if (player.UserIDString == userID) return player;
            return null;
        }

        public IPlayer FindPlayer(string nameOrId)
        {
            foreach (var activePlayer in BasePlayer.allPlayerList)
            {
                if (activePlayer.UserIDString == nameOrId)
                    return activePlayer.IPlayer;
                if (activePlayer.displayName.ToLower() == nameOrId.ToLower())
                    return activePlayer.IPlayer;
            }

            return null;
        }

        private Job FindJobWithID(string id)
        {
            var query = from outer in storedData.Jobs
                        from inner in outer.Value
                        where inner.JobID.ToString() == id
                        select inner;

            if (!query.Any()) return null;
            return query.First();
        }

        private Position FindPositionWithID(string id)
        {
            var query = from outer in storedData.Positions
                        from inner in outer.Value
                        where inner.PositionID.ToString() == id
                        select inner;

            if (!query.Any()) return null;
            return query.First();
        }

        private List<Position> FindPositionsWithOwnerID(string ownerID)
        {
            var query = from outer in storedData.Positions
                        from inner in outer.Value
                        where inner.OwnerID.ToString() == ownerID
                        select inner;

            List<Position> positions = new List<Position>();

            if (!query.Any()) return positions;

            foreach(var q in query)
            {
                positions.Add(q);
            }
            
            return positions;
        }

        private float GenericDistance(GenericPosition a, GenericPosition b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        #endregion Helpers

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Future Config Options Here")]
            public bool TempBool = true;

            public string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(ToJson());
        }

        protected override void LoadDefaultConfig() => config = new Configuration();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    throw new JsonException();
                }

                if (!config.ToDictionary().Keys.SequenceEqual(Config.ToDictionary(x => x.Key, x => x.Value).Keys))
                {
                    Puts("Configuration appears to be outdated; updating and saving");
                    SaveConfig();
                }
            }
            catch
            {
                Puts($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Puts($"Configuration changes saved to {Name}.json");
            Config.WriteObject(config, true);
        }

        #endregion Configuration

        #region Data

        private class StoredData
        {
            public Dictionary<string, List<Job>> Jobs = new Dictionary<string, List<Job>>();
            public Dictionary<string, List<Position>> Positions = new Dictionary<string, List<Position>>();

            public StoredData()
            {
            }

            public void ClearJobs()
            {
            	Jobs.Clear();
            }

            public void ClearPositions()
            {
            	Positions.Clear();
            }

            public void Clear()
            {
            	ClearJobs();
            	ClearPositions();
            }
        }

        private class Job
        {
        	public static double CurrentID = 0;

        	public double ID { get; set; }
        	public string Name { get; set; }
        	public string Description { get; set; }
        	public double PayRate { get; set; }
        	public string AbilityGroup { get; set; }

        	[JsonConstructor]        	
        	public Job(double id, string name, string description, double payRate, string abilityGroup)
        	{
        		ID = id;
        		Name = name;
        		Description = description;
        		PayRate = payRate;
        		AbilityGroup = abilityGroup;
        	}

        	public Job(string name, string description, double payRate, string abilityGroup) : this(++CurrentID, name, description, payRate, abilityGroup)
        	{        		
        	}

        	public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(this));
        }

        private class Position
        {
        	public static double CurrentID = 0;

        	public bool Filled { get; set; }
            public double ID { get; set; }
        	public Job Title { get; set; }
        	public Job ReportsTo { get; set; }
        	public List<Job> Reports { get; set; }
        	public float ClockInTime { get; set; }
        	public float ClockOutTime { get; set; }
        	public float Paycheck { get; set; }
        	public bool ClockedIn { get; set; }

        	[JsonConstructor]
        	public Position(bool filled, double id, Job title, Job reportsTo, List<Job> reports, float clockInTime, float clockOutTime, float paycheck)
        	{
        		Filled = filled;
                ID = id;
        		Title = title;
        		ReportsTo = reportsTo;
        		Reports = reports;
        		ClockInTime = clockInTime;
        		ClockOutTime = clockOutTime;
        		Paycheck = paycheck;
        		ClockedIn = false;
        	}

        	public Position(Job title) : this(false, ++CurrentID, title, null, new List<Job>(), 0f, 0f, 0f)
        	{
        	}

        	public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(this));
        }

        private enum JobEditField
        {
            NAME,
            DESCRIPTION,
            PAYRATE,
            GROUP
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        #endregion Data
	}
}