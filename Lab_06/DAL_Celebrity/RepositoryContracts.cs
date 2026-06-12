namespace DAL_Celebrity;

public interface IRepository<TCelebrity, TLifeevent> :
    IMix<TCelebrity, TLifeevent>,
    ICelebrity<TCelebrity>,
    ILifeevent<TLifeevent>
{
}

public interface IMix<TCelebrity, TLifeevent>
{
    List<TLifeevent> GetLifeeventsByCelebrityId(int celebrityId);
    TCelebrity? GetCelebrityByLifeeventId(int lifeeventId);
}

public interface ICelebrity<T> : IDisposable
{
    List<T> GetAllCelebrities();
    T? GetCelebrityById(int id);
    bool DelCelebrity(int id);
    bool AddCelebrity(T celebrity);
    bool UpdCelebrity(int id, T celebrity);
    int GetCelebrityIdByName(string name);
}

public interface ILifeevent<T> : IDisposable
{
    List<T> GetAllLifeevents();
    T? GetLifeevetById(int id);
    bool DelLifeevent(int id);
    bool AddLifeevent(T lifeevent);
    bool UpdLifeevent(int id, T lifeevent);
}
