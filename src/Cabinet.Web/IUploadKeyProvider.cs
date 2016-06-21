namespace Cabinet.Web {
    public interface IUploadKeyProvider : IKeyProvider {
        string NormalizeKey(string key);
    }
}