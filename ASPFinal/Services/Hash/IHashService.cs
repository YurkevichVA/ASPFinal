namespace ASPFinal.Services.Hash
{
    public interface IHashService
    {
        /// <summary>
        /// Обчислює геш від рядкового аргументу та подає його у гексадецимальному вигляді
        /// </summary>
        /// <param name="text">Вхідний текст</param>
        /// <returns>Гексадецимальний рядок з геш-образом тексту</returns>
        string Hash(string text);
    }
}
