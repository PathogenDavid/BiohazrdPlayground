class MyBaseClass
{
    virtual void VirtualMethod() = 0;
};

class MyChildClass : public MyBaseClass
{
    void ChildMethod() {}
};
