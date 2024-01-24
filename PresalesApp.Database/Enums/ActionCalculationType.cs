namespace PresalesApp.Database.Enums;

///<summary>Способ подсчёта рангов за действие</summary>
public enum ActionCalculationType
{
    ///<summary>Суммирование</summary>
    Sum,

    ///<summary>Первое вхождение</summary>
    Unique,

    ///<summary>По затраченному времени</summary>
    TimeSpend,

    ///<summary>Игнорирование</summary>
    Ignore
}
