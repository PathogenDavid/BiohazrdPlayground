class HasConstOverloads
{
public:
    int& GetNumberReference();
    const int& GetNumberReference() const;
};
