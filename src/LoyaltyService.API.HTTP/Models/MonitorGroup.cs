using System;
using System.Collections.Generic;
using LoyaltyService.Common;

namespace LoyaltyService.API.HTTP.Models
{
	public class MonitorItem
	{
		public string Topic { get; set; }
		public DateTime TimeStamp { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
	}

	public class MonitorGroup
	{
		private readonly Dictionary<string, MonitorItem> _items = new Dictionary<string, MonitorItem> ();

		public string Name { get; set; }
		public DateTime TimeStamp { get; set; }
		public List<MonitorItem> Items {
			get { return new List<MonitorItem>(_items.Values); }
		}

		public MonitorGroup ()
		{
			//default constructor
		}

		public MonitorGroup (string name, DateTime timeStamp)
		{
			Ensure.NotNullOrEmpty(name, "name");

			Name = name;
			TimeStamp = timeStamp;
		}

		public MonitorGroup (MonitorItem item)
		{
			Ensure.NotNull(item, "item");

			Name = item.Topic;
			TimeStamp = item.TimeStamp;
			Upsert(item);
		}

		public void Upsert(MonitorItem item)
		{
			if (!_items.ContainsKey (item.Key))
				_items.Add (item.Key, null);

			_items [item.Key] = item;
		}

		public void Update(string key, string value)
		{
			if (!_items.ContainsKey (key))
				throw new InvalidOperationException (string.Format("Key [{0}] does not exist.", key));

			_items [key].Value = value;
		}

		public static MonitorGroup Create(MonitorItem item)
		{
			return new MonitorGroup (item);
		}

		public static MonitorGroup Create(string name, DateTime timeStamp)
		{
			return new MonitorGroup (name, timeStamp);
		}

		public static MonitorGroup Create(string name, DateTime timeStamp, string key, string value)
		{
			return new MonitorGroup (new MonitorItem{
				Topic = name,
				TimeStamp = timeStamp,
				Key = key,
				Value = value,
			});
		}
	}
}

