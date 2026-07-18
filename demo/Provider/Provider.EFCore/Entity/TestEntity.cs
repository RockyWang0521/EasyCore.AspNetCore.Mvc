using EasyCore.EFCoreRepository.EntityBase;

namespace Provider.EFCore.Entity
{
    public class TestEntity : EasyCoreEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }
}
