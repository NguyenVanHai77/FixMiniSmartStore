using MiniSmartstoreMvc.Models;

namespace MiniSmartstoreMvc.Extensions
{
    public static class ProductAvailabilityExtensions
    {
        public static IQueryable<Product> AvailableForSale(
            this IQueryable<Product> query,
            DateTime now)
        {
            return query.Where(p =>
                p.IsActive &&

                (
                    !p.AvailableStartDate.HasValue ||
                    p.AvailableStartDate.Value <= now
                ) &&

                (
                    !p.AvailableEndDate.HasValue ||
                    p.AvailableEndDate.Value > now
                )
            );
        }
    }
}