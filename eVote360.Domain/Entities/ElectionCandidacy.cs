using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Domain.Entities
{
    public class ElectionCandidacy
    {
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public int CandidacyId { get; set; }

        public Election Election { get; set; } = null!;
        public Candidacy Candidacy { get; set; } = null!;
    }
}
