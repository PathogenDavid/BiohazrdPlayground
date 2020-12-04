struct TestStructNoPadding
{
    int FieldA : 2;
    int FieldB : 2;
    int FieldC : 2;
};

struct TestStruct
{
    int FieldA : 2;
    int : 2; // <-- Explicit padding field
    int FieldB : 2;
    int : 0; // <-- Explicit alignment padding field
    int FieldC : 2;
};
