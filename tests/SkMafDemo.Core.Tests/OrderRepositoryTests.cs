using FluentAssertions;
using SkMafDemo.Core.Domain;

namespace SkMafDemo.Core.Tests;

public class OrderRepositoryTests
{
    [Fact]
    public void Seeded_repository_contains_five_orders()
    {
        var repo = new OrderRepository();
        repo.All().Should().HaveCount(5);
    }

    [Fact]
    public void Find_is_case_insensitive_and_returns_matching_order()
    {
        var repo = new OrderRepository();
        repo.Find("ord-10432").Should().NotBeNull()
            .And.Subject.As<Order>().CustomerName.Should().Be("Alan Turing");
    }

    [Fact]
    public void Find_returns_null_for_unknown_id()
    {
        var repo = new OrderRepository();
        repo.Find("ORD-99999").Should().BeNull();
    }

    [Fact]
    public void ORD_10432_is_seeded_to_be_late_relative_to_today()
    {
        var repo = new OrderRepository();
        var order = repo.Find("ORD-10432")!;
        order.PromisedBy.Should().BeBefore(repo.Today(),
            because: "the side-by-side demos need a deliberately late order");
        order.Status.Should().Be(OrderStatus.Shipped,
            because: "a shipped-but-late order is the interesting case");
    }
}
