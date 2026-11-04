namespace ProjectContractsSignalR.Hubs;

/// <summary>Методи, які сервер викликає на клієнтах (строго типізований хаб).</summary>
public interface IContractsClient
{
    /// <summary>Дані змінено: entity — таблиця, message — опис, author — ім'я користувача, що вніс зміну.</summary>
    Task DataChanged(string entity, string message, string author, string connectionId);

    /// <summary>Кількість підключених клієнтів.</summary>
    Task OnlineCount(int count);
}
