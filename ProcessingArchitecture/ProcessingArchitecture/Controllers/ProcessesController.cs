using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProcessingArchitecture.Controllers
{
    public class ProcessesController : ApiController
    {
        public ProcessCollectionState Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return new ProcessCollectionState
            {
                Processes = Process
                    .GetProcessesByName(name)
                    .Select(p => new ProcessState(p))
            };
        }
    }

    public class ProcessState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TotalProcessorTimeInMillis { get; set; }

        public ProcessState() { }
        public ProcessState(Process proc)
        {
            Id = proc.Id;
            Name = proc.ProcessName;
            TotalProcessorTimeInMillis = proc.TotalProcessorTime.TotalMilliseconds;
        }
    }

    public class ProcessCollectionState
    {
        public IEnumerable<ProcessState> Processes { get; set; }
    }
}
