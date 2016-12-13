using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZYSocket.MicroThreading
{
    public class ScriptSystem
    {
        public Scheduler scheduler { get; set; }
        private CancellationTokenSource cancelTokenSource;

        private readonly HashSet<Script> registeredScripts = new HashSet<Script>();
        private readonly HashSet<Script> scriptsToStart = new HashSet<Script>();
        private readonly HashSet<SyncScript> syncScripts = new HashSet<SyncScript>();
        private readonly List<Script> scriptsToStartCopy = new List<Script>();
        private readonly List<SyncScript> syncScriptsCopy = new List<SyncScript>();

    
        public int SleepTime { get; set; }

        public int LowTime { get; set; }

        public ScriptSystem(int sleepTime=1)
        {
            SleepTime = sleepTime;
           
            scheduler = new Scheduler();
        }

        public void Start()
        {
            cancelTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(Run, cancelTokenSource.Token);
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
        }

        private void Run()
        {
            while (!cancelTokenSource.IsCancellationRequested)
            {
                if (registeredScripts.Count > 0)
                {
                    // Copy scripts to process (so that scripts added during this frame don't affect us)
                    // TODO: How to handle scripts that we want to start during current frame?
                    scriptsToStartCopy.AddRange(scriptsToStart);
                    scriptsToStart.Clear();
                    syncScriptsCopy.AddRange(syncScripts);

                    // Schedule new scripts: StartupScript.Start() and AsyncScript.Execute()
                    foreach (var script in scriptsToStartCopy)
                    {
                        // Start the script
                        var startupScript = script as StartupScript;
                        if (startupScript != null)
                        {
                            scheduler.Add(startupScript.Start, startupScript.Priority);
                        }
                        else
                        {
                            // Start a microthread with execute method if it's an async script
                            var asyncScript = script as AsyncScript;
                            if (asyncScript != null)
                            {
                                asyncScript.MicroThread = AddTask(asyncScript.Execute, asyncScript.Priority);
                            }
                        }
                    }

                    // Schedule existing scripts: SyncScript.Update()
                    foreach (var syncScript in syncScriptsCopy)
                    {
                        // Update priority
                        var updateSchedulerNode = syncScript.UpdateSchedulerNode;
                        updateSchedulerNode.Value.Priority = syncScript.Priority;

                        // Schedule
                        scheduler.Schedule(updateSchedulerNode, ScheduleMode.Last);
                    }


                    // Run current micro threads
                    scheduler.Run();


                    // Flag scripts as not being live reloaded after starting/executing them for the first time                 

                    scriptsToStartCopy.Clear();
                    syncScriptsCopy.Clear();

                    if(SleepTime>0)
                        System.Threading.Thread.Sleep(SleepTime);
                }
                else
                    System.Threading.Thread.Sleep(10);
            }
        }




        public void Add(Script script)
        {
           
            registeredScripts.Add(script);

            // Register script for Start() and possibly async Execute()
            scriptsToStart.Add(script);

            // If it's a synchronous script, add it to the list as well
            var syncScript = script as SyncScript;
            if (syncScript != null)
            {
                syncScript.UpdateSchedulerNode = scheduler.Create(syncScript.Update, syncScript.Priority);
                syncScript.UpdateSchedulerNode.Value.Token = syncScript;
                syncScripts.Add(syncScript);
            }
        }


        public void Remove(Script script)
        {
            // Make sure it's not registered in any pending list
            var startWasPending = scriptsToStart.Remove(script);
            var wasRegistered = registeredScripts.Remove(script);

            if (!startWasPending && wasRegistered)
            {
                // Cancel scripts that were already started
                try
                {
                    script.Cancel();
                }
                catch (Exception e)
                {
                    throw e;
                }

                var asyncScript = script as AsyncScript;
                asyncScript?.MicroThread.Cancel();
            }

            var syncScript = script as SyncScript;
            if (syncScript != null)
            {
                syncScripts.Remove(syncScript);
                syncScript.UpdateSchedulerNode = null;
            }
        }

        public MicroThread AddTask(Func<Task> microThreadFunction, int priority = 0)
        {
            var microThread = scheduler.Create();
            microThread.Priority = priority;
            microThread.Start(microThreadFunction);
            return microThread;
        }
    }
}
