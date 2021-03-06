using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgresEntities.Entities
{
    [Table("servers")]
    public class Server
    {
        [Key]
        [Column("address", Order = 0)]
        public string Address { get; set; }

        [Key]
        [Column("port", Order = 1)]
        public int Port { get; set; }
        
        [Column("ping_port")]
        public int PingPort { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("info")]
        public string Info { get; set; }
    }
}
