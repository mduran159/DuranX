namespace Cart.API.Exceptions;

public class CartNotFoundException : NotFoundException
{
    public CartNotFoundException(string userName) : base("Cart", userName)
    {
        
    }
}
 