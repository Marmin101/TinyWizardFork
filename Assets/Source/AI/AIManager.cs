using System.Collections.Generic;
using UnityEngine;

namespace Quinn.AI
{
	public class AIManager : MonoBehaviour
	{
		public static AIManager Instance { get; private set; }

		public IEnumerable<AIAgent> AllAgents => _allAgents;
		public IEnumerable<AIAgent> EnvironmentAgents => _environmentAgents;
        // Fixed a bug where the following 3 enumerables were incorrectly set to _environmentAgents.
        public IEnumerable<AIAgent> MonsterAgents => _monsterAgents;
		public IEnumerable<AIAgent> CultistAAgents => _cultistAAgents;
		public IEnumerable<AIAgent> CultistBAgents => _cultistBAgents;

		private readonly List<AIAgent> _allAgents = new();
		private readonly List<AIAgent> _environmentAgents = new();
		private readonly List<AIAgent> _monsterAgents = new();
		private readonly List<AIAgent> _cultistAAgents = new();
		private readonly List<AIAgent> _cultistBAgents = new();
        // These Lists used to be HashSets.
        // In this case, HashSets are more efficient than lists,
        // because they have time complexity O(1), whereas Lists have time complexity O(n).
        // Time complexity O(1) means it is constant time, no matter the size of the container.
        // Time complexity O(n) means that it is linear time, taking more time as the size of the container increases.
        // In this game, there are not many enemies loaded at once, so there is no noticeable benefit to either-or.
        // Lists in this context are also better because the order of the elements is important.
        // HashSets do not maintain the order of the elements, whereas Lists do.
		// The order is important to know which agents are being operated on (added or removed or indexed).

        private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void AddAgent(AIAgent agent)
		{
			_allAgents.Add(agent);

			switch (agent.Health.Team)
			{
				case Team.Environment:
				{
					_environmentAgents.Add(agent);
					break;
				}
				case Team.Monster:
				{
					_monsterAgents.Add(agent);
					break;
				}
				case Team.CultistA:
				{
					_cultistAAgents.Add(agent);
					break;
				}
				case Team.CultistB:
				{
					_cultistBAgents.Add(agent);
					break;
				}
			}
		}

		public void RemoveAgent(AIAgent agent)
		{
			_allAgents.Remove(agent);

			switch (agent.Health.Team)
			{
				case Team.Environment:
				{
					_environmentAgents.Remove(agent);
					break;
				}
				case Team.Monster:
				{
					_monsterAgents.Remove(agent);
					break;
				}
				case Team.CultistA:
				{
					_cultistAAgents.Remove(agent);
					break;
				}
				case Team.CultistB:
				{
					_cultistBAgents.Remove(agent);
					break;
				}
			}
		}
	}
}
