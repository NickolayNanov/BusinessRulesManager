namespace BusinessRulesManager.Data.Contracts
{
    public interface IEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
