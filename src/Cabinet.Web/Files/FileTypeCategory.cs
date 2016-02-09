using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Web.Files {
    public enum FileTypeCategory {
        Unknown = 0,
        Document = 1,
        Image = 2,
		Audio = 3,
        Video = 4,
        Archive = 5,
        Html = 6,
        Code = 7
    }
}
