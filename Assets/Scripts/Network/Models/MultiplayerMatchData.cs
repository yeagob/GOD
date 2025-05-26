using System;
using System.Collections.Generic;

namespace Network.Models
{
    [Serializable]
    public class MultiplayerMatchData
    {
        public string roomId;
        public string state;
        public string hostId;
        public List<string> players = new List<string>();
        public DateTime createdAt;
        public string boardId;

        public MultiplayerMatchData()
        {
            createdAt = DateTime.UtcNow;
        }

        public MultiplayerMatchData(string roomId, string state, string hostId = null)
        {
            this.roomId = roomId;
            this.state = state;
            this.hostId = hostId;
            this.createdAt = DateTime.UtcNow;
        }
    }
}
