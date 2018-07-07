using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeProcessPriority
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!ProcessArgs(args)) { return; }

			try {
				MainMain();
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		static HashSet<string> ProcessNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		static ProcessPriorityClass? Priority = null;
		static bool DaemonMode = false;
		static bool ListMode = false;
		static bool Verbose = false;
		static Timer TheTimer = null;

		static void MainMain()
		{
			if (DaemonMode && !ListMode)
			{
				var callBack = new TimerCallback(SetPriority);
				using (TheTimer = new Timer(callBack,null,new TimeSpan(0,0,1),new TimeSpan(0,0,1)))
				{
					Console.WriteLine("Running in daemon mode. Press any key to exit");
					Console.ReadKey(true);
				}
			}
			else
			{
				SetPriority(null);
			}
		}

		static void SetPriority(object state)
		{
			var list = Process.GetProcesses();
			bool first = true;
			foreach(Process p in list)
			{
				try {
					if (ListMode && (ProcessNames.Count == 0 || ProcessNames.Contains(p.ProcessName)))
					{
						if (first) {
							Console.WriteLine("Id\tProcess\tPriority");
							first = false;
						}
						Console.WriteLine(p.Id+"\t"+p.ProcessName+"\t"+p.PriorityClass);
					}
					else if (ProcessNames.Contains(p.ProcessName) && p.PriorityClass != Priority)
					{
						if (Verbose) {
							Console.WriteLine(p.Id+"\t"+p.ProcessName+"\t"+p.PriorityClass+" => "+Priority.Value);
						}
						p.PriorityClass = Priority.Value;
					}
				} catch(Win32Exception ex) {
					if (Verbose) {
						Console.WriteLine("WARNING: "+p.ProcessName+" - "+ex.Message);
					}
				} catch(InvalidOperationException ex) {
					if (Verbose) {
						Console.WriteLine("WARNING: "+p.ProcessName+" - "+ex.Message);
					}
				}
			}
		}

		static bool ProcessArgs(string[] args)
		{
			if (args.Length < 1)
			{
				Usage();
				return false;
			}

			ProcessPriorityClass ppc;

			for(int a=0; a<args.Length; a++)
			{
				string c = args[a];
				if (c == "-d") {
					DaemonMode = true;
				} else if (c == "-l") {
					ListMode = true;
				} else if (c == "-v") {
					Verbose = true;
				} else if (Enum.TryParse(c,true,out ppc)) {
					Priority = ppc;
				} else {
					ProcessNames.Add(c);
				}
			}

			if (!ListMode && Priority == null) {
				Console.WriteLine("ERROR: You must provide a priority");
				Usage();
				return false;
			}
			else if (!ListMode && ProcessNames.Count < 1) {
				Console.WriteLine("ERROR: You must specify at least one process name");
				Usage();
				return false;
			}
			return true;
		}

		static void Usage()
		{
			StringBuilder sb = new StringBuilder();
			var names = Enum.GetNames(typeof(ProcessPriorityClass));
			foreach(string n in names) {
				sb.AppendLine().Append("  ").Append(n);
			}

			Console.WriteLine(""
				+ nameof(ChangeProcessPriority) + " [options] (priority) (list of process names)"
				+"\n Priorities:"
				+sb.ToString()
				+"\n Options:"
				+"\n  -l  list information about processes"
				+"\n  -v  verbose mode - show additioal information and warnings"
				+"\n  -d  daemon mode - continues to watch for new instances of the"
				+"\n      process(s) and changes the priority for each one"
			);
		}
	}
}
