using KeeperBaseLib.Model;
namespace KeeperDataGridExample;

public class TableModel : PropertyModel
{
    private string? _name;
    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            RaisePropertyChanged();
        }
    }

    private int? _age;
    public int? Age
    {
        get => _age;
        set
        {
            _age = value;
            RaisePropertyChanged();
        }
    }

    private string? _country;
    public string? Country
    {
        get => _country;
        set
        {
            _country = value;
            RaisePropertyChanged();
        }
    }

    private int? _position;
    public int? Position
    {
        get => _position;
        set
        {
            _position = value;
            RaisePropertyChanged();
        }
    }
}