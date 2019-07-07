﻿using Microsoft.Extensions.Logging;
using Rhisis.Core.Data;
using Rhisis.Core.Extensions;
using Rhisis.Core.Resources.Include;
using Rhisis.Core.Structures.Game;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rhisis.Core.Resources.Loaders
{
    public sealed class JobLoader : IGameResourceLoader
    {
        private readonly ILogger<JobLoader> _logger;
        private readonly IDictionary<int, JobData> _jobsData;
        private readonly DefineLoader _defines;

        /// <summary>
        /// Gets job by his id.
        /// </summary>
        /// <param name="jobId">Job Id</param>
        /// <returns></returns>
        public JobData this[int jobId] => this.GetJob(jobId);

        /// <summary>
        /// Creates a new <see cref="JobLoader"/> instance.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="defines">Defines</param>
        public JobLoader(ILogger<JobLoader> logger, DefineLoader defines)
        {
            this._logger = logger;
            this._jobsData = new Dictionary<int, JobData>();
            this._defines = defines;
        }

        /// <inheritdoc />
        public void Load()
        {
            if (!File.Exists(GameResources.JobPropPath))
            {
                this._logger.LogWarning($"Unable to load job properties. Reason: cannot find '{GameResources.JobPropPath}' file.");
                return;
            }

            using (var propJob = new ResourceTableFile(GameResources.JobPropPath, -1, new[] { '\t', ' ', '\r' }, this._defines.Defines, null))
            {
                var jobs = propJob.GetRecords<JobData>();

                foreach (var job in jobs)
                {
                    if (this._jobsData.ContainsKey(job.Id))
                    {
                        this._jobsData[job.Id] = job;
                        this._logger.LogWarning(GameResources.ObjectOverridedMessage, "JobData", job.Id, "already delcared");
                    }
                    else
                        this._jobsData.Add(job.Id, job);
                }
            }

            using (var etcFile = new IncludeFile(GameResources.EtcIncPath))
            {
                Block jobBlock = etcFile.GetBlock("job");

                if (jobBlock != null)
                {
                    IEnumerable<IGrouping<int, string>> groups = jobBlock.UnknownStatements.GroupBy(4);

                    foreach (var group in groups)
                    {
                        int jobId = this._defines.Defines[group.ElementAt(0)];

                        if (this._jobsData.TryGetValue(jobId, out JobData jobData))
                        {
                            jobData.Name = group.ElementAt(1);
                            jobData.BaseJob = (DefineJob.Job)this._defines.Defines[group.ElementAt(2)];
                            jobData.Type = (DefineJob.JobType)this._defines.Defines[group.ElementAt(3)];
                        }
                    }
                }
            }

            this._logger.LogInformation($"-> {this._jobsData.Count} jobs data loaded.");
        }

        /// <summary>
        /// Gets job by his id.
        /// </summary>
        /// <param name="jobId">Job Id</param>
        /// <returns></returns>
        public JobData GetJob(int jobId) => this._jobsData.TryGetValue(jobId, out JobData value) ? value : null;

        /// <inheritdoc />
        public void Dispose()
        {
            this._jobsData.Clear();
        }
    }
}
