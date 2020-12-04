class MyBaseClass
{
    virtual void VirtualMethod() = 0;
};

class MyChildClass : public MyBaseClass
{
    virtual void ChildVirtualMethod() = 0;
};
