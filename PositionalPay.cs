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

        [PluginReference]
        private Plugin Economics;

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
                ["PersonNotFound"] = "Person not found",
                ["Separator"] = "-----------------------------",

                ["PosInfo0"] = "A position consists of a position ID, job title, which job title they report to, which "
                             + "job titles report to them, current clock-in/clock-out times, and current accumulated paycheck.",
                ["PosInfo1"] = "Use '/position list' to view your positions.",
                ["PosInfo2"] = "Use '/position create JobID#' to create a new position using job with id JobID#.\nUse '/position delete PosID#' to delete position with ID number PosID#.",
                ["PosInfo3"] = "Use '/position clockin PosID#' to Clock-In for your position with ID number PosID#.\n"
                             + "Use '/position clockout PosID#' to Clock-Out from your position with ID number PosID#.",
                ["PosInfo4"] = "Use '/position getpaycheck' to receive pay in coin that has accumulated in your paycheck for all of your positions.",
                ["PosInfo5"] = "Use '/position quit PosID#' to quit your position with ID number PosID#. You forfeit any unclaimed pay.",
                ["PosInfo6"] = "Use '/position hire PosID# Name' to hire Name for the position with ID number PosID#.\n"
                             + "Use '/position fire PosID# Name' to fire Name from the position with ID number PosID#.",
                ["PosInfo7"] = "Use '/position edit PosID# FieldName NewValue' to edit the field FieldName of the position with ID PosID# to NewValue.\n"
                             + "Field Names: JOBID, REPORT_TO, REPORTS_ADD, REPORTS_REMOVE",
                ["PosInfo8"] = "Use '/position details PosID#' to view details about the position with ID number PosID#.",
                ["PosNotFound"] = "Cannot find a position with id #{0}",

                ["PosListHeader"] = "<color=#00D8D8>YOUR POSITIONS</color>",
                ["ClockedIN"] = "<color=#66FF00>CLOCKED-IN</color>",
                ["ClockedOUT"] = "<color=#FF0000>CLOCKED-OUT</color>",

                ["PosDetails"] = "ID: {0}\n" +
                                 "Title: {1}" +
                                 "Reports To: {2}" +
                                 "Reports: {3}" +
                                 "Clocked In: {4}" +
                                 "Current Paycheck: {5}",

                ["ClockInSuccess"] = "You successfully <color=#66FF00>CLOCKED-IN</color> to position #{0}",
                ["ClockOutSuccess"] = "You successfully <color=#FF0000>CLOCKED-OUT</color> of position #{0}. Your pay has been added to your paycheck. Use /pos getpaycheck to get paid.",

                ["PosCreateSuccess"] = "A new position with id #{0} was created",
                ["PosDeleteSuccess"] = "The position with id #{0} was deleted",

                ["PosHireCurrentlyFilled"] = "This position is currently filled. The current employee must be fired before someone can be hired.",
                ["PosHireSuccess"] = "{0} has been successfully hired for the position with id #{1}.",

                ["PosFireCurrentlyUnfilled"] = "This position is currently unfilled.",
                ["PosFireSuccess"] = "{0} has been successfully fire from the position with id #{1}",

                ["PosQuitFail"] = "You don't work that position.",
                ["PosQuitSuccess"] = "You have successfully quit position #{0} and forfeited any unclaimed paychecks.",

                ["PaycheckSuccess"] = "{0} coin has been placed in your pocket from your paycheck for position with id #{1}.",

                ["PosEditFailure"] = "Position #{0} edit failure",
                ["PosEditSuccess"] = "Position #{0} edit success",

                ["JobInfo0"] = "A job title consists of a job ID, name, description, hourly payrate, and an ability group.",
                ["JobInfo1"] = "Use '/job list' to view jobs.",
                ["JobInfo2"] = "Use '/job create Name \"Description\" Payrate# AbilityGroup' to create a job called Name, " 
                             + "described by Description, paid Payrate# hourly, with abilities from AbilityGroup.",
                ["JobInfo3"] = "Use '/job delete JobID#' to permanently remove the job with ID number JobID#.",
                ["JobInfo4"] = "Use '/job edit JobID# FieldName NewValue' to edit the field FieldName of the job with ID number PosID# to NewValue.\n"
                             + "Field Names: NAME, DESCRIPTION, PAYRATE, GROUP",
                ["JobInfo5"] = "Use '/job details JobID#' to view details about the position with ID number JobID#.",

                ["JobDetails"] = "ID: {0}\n" +
                                 "Name: {1}" +
                                 "Description: {2}" +
                                 "Payrate: {3}" +
                                 "Ability Group: {4}",

                ["JobCreateSuccess"] = "You have successfully created the {0} job with id #{1}",
                ["JobNotFound"] = "Cannot find a job with id #{0}",
                ["JobDeleteSuccess"] = "You have successfully deleted the {0} job with id #{1}",
                ["JobEditFailure"] = "Job #{0} edit failure",
                ["JobEditSuccess"] = "You have successfully updated {0} to {1} for Job #{2}"
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
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermUse)) message += "|list|details|clockin|clockout|getpaycheck|quit";
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermManage)) message += "|create|delete|hire|fire|edit";

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
        		    var yourPositions = FindPositionsWithOwnerID(iPlayer.Id);

        		    if (yourPositions.IsEmpty())
        		    {
        		    	iPlayer.Reply(Lang("NoPositions", iPlayer.Id, command));
                        return;
        		    }

        		    iPlayer.Reply(Lang("PosListHeader", iPlayer.Id, command));
        		    iPlayer.Reply(Lang("Separator", iPlayer.Id, command));

        		    string posList = "";

        		    foreach (Position p in yourPositions)
        		    {
        		    	if (p.Filled)
        		    	{
        		    		posList += p.Title.Name + ", ID #" + p.ID + "\n";
        		    	}
        		    }

        		    iPlayer.Reply(posList);

        		    return;

                case "details":
                    if (args.Length < 2)
                    {
                        iPlayer.Reply(Lang("PosInfo8", iPlayer.Id, command));
                        return;
                    }

                    Position viewPos = FindPositionWithID(args[1]);

                    if (viewPos == null)
                    {
                        iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
                    }

                    iPlayer.Reply(Lang("PosDetails", iPlayer.Id, viewPos.ID, viewPos.Title.Name, viewPos.ReportsTo.Name, String.Join(", ", viewPos.Reports.Select(x=>x.Name).ToArray()), viewPos.ClockedIn, viewPos.Paycheck));

                    return;

        		case "clockin":
        		    if (args.Length < 2)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo3", iPlayer.Id, command));
                        return;
        		    }

        		    Position clockinPos = FindPositionWithID(args[1]);

        		    if (clockinPos == null)
        		    {
        		    	iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    clockinPos.ClockIn();

        		    iPlayer.Reply(Lang("ClockInSuccess", iPlayer.Id, clockinPos.ID));

        		    return;

        		case "clockout":
        		    if (args.Length < 2)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo3", iPlayer.Id, command));
                        return;
        		    }

        		    Position clockoutPos = FindPositionWithID(args[1]);

        		    if (clockoutPos == null)
        		    {
        		    	iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    clockoutPos.ClockOut();

        		    iPlayer.Reply(Lang("ClockOutSuccess", iPlayer.Id, clockoutPos.ID));

        		    return;

        		case "getpaycheck":
                    var payPositions = FindPositionsWithOwnerID(iPlayer.Id);

                    if (payPositions.IsEmpty())
                    {
                        iPlayer.Reply(Lang("NoPositions", iPlayer.Id, command));
                        return;
                    }

                    foreach (Position p in payPositions)
                    {
                        iPlayer.Reply(Lang("PaycheckSuccess", iPlayer.Id, p.Paycheck, p.ID));
                        Economics.Call<bool>("Deposit", iPlayer.Id, p.Paycheck);
                        p.Paycheck = 0f;
                    }

        		    return;

        		case "quit":
        		    if (args.Length < 2)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo5", iPlayer.Id, command));
                        return;
        		    }

        		    Position quitPos = FindPositionWithID(args[1]);

        		    if (quitPos == null)
        		    {
        		    	iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    IPlayer newQuit = FindPlayer(iPlayer.Id);

        		    if (newQuit == null)
        		    {
        		    	iPlayer.Reply(Lang("PersonNotFound", iPlayer.Id));
                        return;
        		    }

        		    if (!quitPos.Filled)
        		    {
        		    	iPlayer.Reply(Lang("PosQuitFail", iPlayer.Id, command));
                        return;
        		    }

        		    quitPos.Quit();

        		    iPlayer.Reply(Lang("PosQuitSuccess", iPlayer.Id, quitPos.ID));

        		    return;

        		case "create":
        		    if (args.Length < 2)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo2", iPlayer.Id, command));
                        return;
        		    }

        		    Job title = FindJobWithID(args[1]);

        		    if (title == null)
        		    {
        		    	iPlayer.Reply(Lang("JobNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    Position newPos = new Position(title, iPlayer.Id);

        		    if (storedData.Positions.ContainsKey(iPlayer.Id))
                    {
                        storedData.Positions[iPlayer.Id].Add(newPos);
                    }
                    else
                    {
                        storedData.Positions.Add(iPlayer.Id, new List<Position>() { newPos });
                    }

                    iPlayer.Reply(Lang("PosCreateSuccess", iPlayer.Id, newPos.ID));

        		    return;

        		case "delete":
        		    if (args.Length < 2)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo2", iPlayer.Id, command));
                        return;
        		    }

        		    Position delPos = FindPositionWithID(args[1]);

        		    if (delPos == null)
        		    {
        		    	iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    iPlayer.Reply(Lang("PosDeleteSuccess", iPlayer.Id, args[1]));
                    storedData.Positions[delPos.OwnerID].Remove(delPos);

        		    return;

        		case "hire":
        		    if (args.Length < 3)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo6", iPlayer.Id, command));
                        return;
        		    }

        		    Position hirePos = FindPositionWithID(args[1]);

        		    if (hirePos == null)
        		    {
        		    	iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    IPlayer newHire = FindPlayer(args[2]);

        		    if (newHire == null)
        		    {
        		    	iPlayer.Reply(Lang("PersonNotFound", iPlayer.Id));
                        return;
        		    }

        		    if (hirePos.Filled)
        		    {
        		    	iPlayer.Reply(Lang("PosHireCurrentlyFilled", iPlayer.Id, command));
                        return;
        		    }

        		    hirePos.Hire(newHire.Id);

        		    iPlayer.Reply(Lang("PosHireSuccess", iPlayer.Id, (newHire.Object as BasePlayer).displayName, hirePos.ID));

        		    return;

        		case "fire":
        		    if (args.Length < 3)
        		    {
        		    	iPlayer.Reply(Lang("PosInfo6", iPlayer.Id, command));
                        return;
        		    }

        		    Position firePos = FindPositionWithID(args[1]);

        		    if (firePos == null)
        		    {
        		    	iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, args[1]));
                        return;
        		    }

        		    IPlayer newFire = FindPlayer(args[2]);

        		    if (newFire == null)
        		    {
        		    	iPlayer.Reply(Lang("PersonNotFound", iPlayer.Id));
                        return;
        		    }

        		    if (!firePos.Filled)
        		    {
        		    	iPlayer.Reply(Lang("PosFireCurrentlyUnfilled", iPlayer.Id, command));
                        return;
        		    }

        		    firePos.Fire(iPlayer.Id);

        		    iPlayer.Reply(Lang("PosFireSuccess", iPlayer.Id, (newFire.Object as BasePlayer).displayName, firePos.ID));

        		    return;

        		case "edit":
                    if(args.Length < 4)
                    {
                        iPlayer.Reply(Lang("PosInfo6", iPlayer.Id, command));
                        return;
                    }

                    Position editPos = FindPositionWithID(args[1]);

                    if (editPos == null)
                    {
                        iPlayer.Reply(Lang("PosNotFound", iPlayer.Id, command));
                        return;
                    }

                    PosEditField posField;
                    if(!PosEditField.TryParse(args[2].ToUpper(), out posField))
                    {
                        iPlayer.Reply(Lang("PosInfo6", iPlayer.Id, command));
                        return;
                    }

                    if(!EditPos(editPos, posField, args[3]))
                    {
                        iPlayer.Reply(Lang("PosEditFailure", iPlayer.Id, command, editPos.ID));
                        return;
                    }

                    iPlayer.Reply(Lang("PosEditSuccess", iPlayer.Id, args[1]));

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
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermUse)) message += "|list|details";
                if (iPlayer.HasPermission(PermAdmin) || iPlayer.HasPermission(PermManage)) message += "|create|delete|edit";

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

                    
                    for(int i = 0; i < Job.CurrentID; i++)
                    {
                        Job currentJob = FindJobWithID(i.ToString());
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
                        jobsOutput += job.Name + " (ID # " + job.ID + ")\n";
                    }

                    iPlayer.Reply(jobsOutput);

        		    return;

                case "details":
                    if (args.Length < 2)
                    {
                        iPlayer.Reply(Lang("PosInfo8", iPlayer.Id, command));
                        return;
                    }

                    Job viewJob = FindJobWithID(args[1]);

                    if (viewJob == null)
                    {
                        iPlayer.Reply(Lang("JobNotFound", iPlayer.Id, args[1]));
                        return;
                    }

                    iPlayer.Reply(Lang("JobDetails", iPlayer.Id, viewJob.ID, viewJob.Name, viewJob.Description, viewJob.PayRate, viewJob.AbilityGroup));

                    return;

        		case "create":
                    if(args.Length < 5)
                    {
                        iPlayer.Reply(Lang("JobInfo2", iPlayer.Id, command));
                        return;
                    }

                    Job newJob = new Job(args[1], args[2], Convert.ToDouble(args[3]), args[4]);

                    storedData.Jobs.Add(newJob.ID.ToString(), new List<Job>() { newJob });

                    iPlayer.Reply(Lang("JobCreateSuccess", iPlayer.Id, newJob.Name, newJob.ID));

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
                        iPlayer.Reply(Lang("JobNotFound", iPlayer.Id, args[1]));
                        return;
                    }

                    iPlayer.Reply(Lang("JobDeleteSuccess", iPlayer.Id, deleteJob.Name, deleteJob.ID));
                    storedData.Jobs.Remove(args[1]);

        		    return;

        		case "edit":
                    if(args.Length < 4)
                    {
                        iPlayer.Reply(Lang("JobInfo4", iPlayer.Id, command));
                        return;
                    }

                    Job editJob = FindJobWithID(args[1]);

                    if (editJob == null)
                    {
                        iPlayer.Reply(Lang("JobNotFound", iPlayer.Id, args[1]));
                        return;
                    }

                    JobEditField jobField;
                    if(!JobEditField.TryParse(args[2].ToUpper(), out jobField))
                    {
                        iPlayer.Reply(Lang("JobInfo4", iPlayer.Id, command));
                        return;
                    }

                    if(!EditJob(editJob, jobField, args[3]))
                    {
                        iPlayer.Reply(Lang("JobEditFailure", iPlayer.Id, command, editJob.ID));
                        return;
                    }

                    string newValue = (jobField == JobEditField.NAME) ? args[3].ToUpper() : args[3];

                    iPlayer.Reply(Lang("JobEditSuccess", iPlayer.Id, jobField.ToString(), newValue, editJob.ID));

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

        private bool EditPos(Position pos, PosEditField field, string value)
        {
            Job job = FindJobWithID(value);
            if(job == null) return false;
            switch(field)
            {
                case PosEditField.JOBID:
                    pos.Title = job;
                    return true;

                case PosEditField.REPORT_TO:
                    pos.ReportsTo = job;
                    return true;

                case PosEditField.REPORTS_ADD:
                    if(pos.Reports.Contains(job)) return false;
                    pos.Reports.Add(job);
                    return true;

                case PosEditField.REPORTS_REMOVE:
                    if(!pos.Reports.Contains(job)) return false;
                    pos.Reports.Remove(job);
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
                        where inner.ID.ToString() == id
                        select inner;

            if (!query.Any()) return null;
            return query.First();
        }

        private Position FindPositionWithID(string id)
        {
            var query = from outer in storedData.Positions
                        from inner in outer.Value
                        where inner.ID.ToString() == id
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

        private List<Position> FindPositionsWithCreatorID(string id)
        {
            var query = from outer in storedData.Positions
                        from inner in outer.Value
                        where inner.CreatorID.ToString() == id
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
        	public DateTime ClockInTime { get; set; }
        	public DateTime ClockOutTime { get; set; }
        	public double Paycheck { get; set; }
        	public bool ClockedIn { get; set; }
        	public string OwnerID { get; set; }
        	public string CreatorID { get; set; }

        	[JsonConstructor]
        	public Position(bool filled, double id, Job title, Job reportsTo, List<Job> reports, DateTime clockInTime, DateTime clockOutTime, float paycheck, string ownerID)
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
        		OwnerID = ownerID;
        		CreatorID = ownerID;
        	}

        	public Position(Job title, string ownerID) : this(false, ++CurrentID, title, null, new List<Job>(), DateTime.MinValue, DateTime.MinValue, 0f, ownerID)
        	{
        	}

        	public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(this));

        	public void Hire(string id)
        	{
        		Filled = true;
        		OwnerID = id;
        	}

        	public void Fire(string id)
        	{
        		Filled = false;
        		OwnerID = id;
        		Paycheck = 0f;
        	}

        	public void Quit()
        	{
        		Filled = false;
        		OwnerID = CreatorID;
        		Paycheck = 0f;
        	}

        	public void ClockIn()
        	{
        		ClockedIn = true;
        		ClockInTime = DateTime.Now;
        	}

        	public void ClockOut()
        	{
        		ClockedIn = false;
        		ClockOutTime = DateTime.Now;
        		if (ClockInTime != DateTime.MinValue)
                {
                    Paycheck += Math.Ceiling(ClockOutTime.Subtract(ClockInTime).Hours * Title.PayRate);
                    ClockInTime = DateTime.MinValue;
                }
        	}
        }

        private enum JobEditField
        {
            NAME,
            DESCRIPTION,
            PAYRATE,
            GROUP
        }

        private enum PosEditField
        {
            JOBID,
            REPORT_TO,
            REPORTS_ADD,
            REPORTS_REMOVE
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        #endregion Data
	}
}