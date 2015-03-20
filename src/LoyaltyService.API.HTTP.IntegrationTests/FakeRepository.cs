using System.Collections.Generic;
using AggregateSource;
using LoyaltyService.Core.Redemption;

namespace LoyaltyService.API.HTTP.IntegrationTests
{
	public class FakeRepository : IRepository<RedemptionProcessManager>
	{
		private readonly Dictionary<string, List<object>> _repository = new Dictionary<string, List<object>>();

		public RedemptionProcessManager Get(string identifier)
		{
			
			var pm = new RedemptionProcessManager();
			if (_repository.ContainsKey(identifier))
			{
				pm.Initialize(_repository[identifier]);
			}

			return pm;
		}

		public Optional<RedemptionProcessManager> GetOptional(string identifier)
		{
			throw new System.NotImplementedException();
		}

		public void Add(string identifier, RedemptionProcessManager root)
		{
			if (root.HasChanges())
			{
				if (!_repository.ContainsKey(identifier))
				{
					_repository.Add(identifier, new List<object>());
				}
				_repository[identifier].AddRange(root.GetChanges());
			}
		}
	}
}