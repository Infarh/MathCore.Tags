namespace MathCore.Tags;

/// <summary>Класс методов-расширений для реализации функциональности добавления объектов, которые могут быть приложены к другим объектам</summary>
public static class TagExtensions
{
    /// <summary>Получить объект-метку указанного типа из целевого объекта</summary>
    /// <typeparam name="TTag">Тип объекта-метки</typeparam>
    /// <param name="o">Целевой объект</param>
    /// <returns>Объект метка, если он существует в указанном объекте</returns>
    public static TTag? GetTag<TTag>(this object o) => TagPool.Tag<TTag>(o);

    /// <summary>Установить объект-метку для указанного объекта</summary>
    /// <typeparam name="TTag">Тип объекта-метки</typeparam>
    /// <param name="o">Целевой объект</param>
    /// <param name="Tag">Объект-метка, прикладываемый к целевому объекту</param>
    public static void SetTag<TTag>(this object o, TTag? Tag) => TagPool.SetTag(o, Tag);
}
