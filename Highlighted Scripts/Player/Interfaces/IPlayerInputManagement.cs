public interface IPlayerInputManagement
{
    void EnableInputFor(string actionInputName, bool value);

    bool Enabled { get; set; }
}
