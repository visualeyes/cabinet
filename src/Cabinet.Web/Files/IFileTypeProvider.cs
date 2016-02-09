using System.Collections.Generic;

namespace Cabinet.Web.Files {
    public interface IFileTypeProvider {
        IEnumerable<IFileType> GetFileTypes();
    }
}