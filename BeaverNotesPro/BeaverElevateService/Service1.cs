using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BeaverElevateService
{
    public partial class Service1 : ServiceBase
    {
        private Thread _workerThread;
        private NamedPipeServer _namedPipeServer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _namedPipeServer = new NamedPipeServer();
            _workerThread = new Thread(_namedPipeServer.Start);
            _workerThread.IsBackground = true;
            _workerThread.Start();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            if (_workerThread != null)
            {
                _workerThread.Abort(); // You might want to handle this more gracefully
                _workerThread.Join();
            }

            base.OnStop();
        }


        private void PerformPrivilegedAction(NamedPipeServerStream pipeServer)
        {
            // Perform privileged actions here
            using (StreamWriter writer = new StreamWriter(pipeServer))
            {
                writer.WriteLine("Action performed.");
            }
        }
    }
}
