using System;
using OpenTable.Services.Components.Monitoring.Monitors.HitTracker;
using LoyaltyService.Common;
using System.Collections.Generic;
using LoyaltyService.API.HTTP.Models;
using System.Linq;

namespace LoyaltyService.API.HTTP.Monitoring
{
	public class MonitorFactory
	{
		private readonly HitTracker _hitTracker;

		public MonitorFactory (HitTracker hitTracker)
		{
			Ensure.NotNull (hitTracker, "hitTracker");
			_hitTracker = hitTracker;
		}

		public ICollection<MonitorGroup> Build()
		{
			var groups = new HashSet<MonitorGroup> ();
			foreach (var endpoint in _hitTracker.GetEndPoints()) {
				groups.Add (CompileHits(endpoint));
			}

			return groups;
		}

		private MonitorGroup CompileHits(string endPointName)
		{
			Hit[] hits = _hitTracker.GetHits(endPointName);
			var hitStats = hits.Aggregate(new HitStats(), (stats, hit) => stats.Calculate(hit));
			MonitorGroup g = new MonitorGroup (endPointName, hitStats.MinStartTime);

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "Count",
				Value = hitStats.Count.ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "ErrorCount",
				Value = hitStats.Errors.ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "LastErrorAt",
				Value = hitStats.LastError.ToString("O"),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "MaxStartTime",
				Value = hitStats.MaxStartTime.ToString("o"),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "MinStartTime",
				Value = hitStats.MinStartTime.ToString("o"),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "MaxTimeTaken (ms)",
				Value = hitStats.MaxTimeTaken.TotalMilliseconds.ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "SumTimeTaken (ms)",
				Value = hitStats.SumTimeTaken.TotalMilliseconds.ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "MeanTimeTaken (ms)",
				Value = (hitStats.SumTimeTaken.TotalMilliseconds / hitStats.Count).ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "WindowElapsedTime (ms)",
				Value = hitStats.MaxStartTime.Subtract(hitStats.MinStartTime).TotalMilliseconds.ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			g.Upsert (new MonitorItem{ 
				Topic = endPointName,
				Key = "Hit Frequency (hits/second)",
				Value = (hitStats.Count / hitStats.MaxStartTime.Subtract(hitStats.MinStartTime).TotalSeconds).ToString(),
				TimeStamp = hitStats.MinStartTime
			});

			return g;
		}
	}

	public class HitStats
	{
		public int Count;
		public int Errors;
		public DateTime LastError;
		public TimeSpan SumTimeTaken;
		public TimeSpan MaxTimeTaken;
		public DateTime MinStartTime;
		public DateTime MaxStartTime;

		public HitStats Calculate(Hit hit)
		{
			Count++;
			Errors += hit.IsError ? 1 : 0;
			LastError = (hit.IsError && hit.StartTime > LastError) ? hit.StartTime : LastError;
			SumTimeTaken += hit.TimeTaken;
			MaxTimeTaken = hit.TimeTaken > MaxTimeTaken ? hit.TimeTaken : MaxTimeTaken;
			MinStartTime = (Count == 1 || hit.StartTime < MinStartTime) ? hit.StartTime : MinStartTime;
			MaxStartTime = hit.StartTime > MaxStartTime ? hit.StartTime : MaxStartTime;
			return this;
		}
	}
}

