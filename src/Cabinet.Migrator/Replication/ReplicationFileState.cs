using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Migrator.Replication {
    public enum ReplicationFileState {
        Same = 0,

        SourceAdded = 1,
        SourceDeleted = 2,
        SourceNewer = 3,

        ReplicationNewer = 4,

    }
}
