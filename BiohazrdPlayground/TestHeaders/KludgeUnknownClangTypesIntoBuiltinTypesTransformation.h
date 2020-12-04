template <class T>
struct Pair
{
    T x;
    T y;
};

struct SomeStruct
{
    Pair<short> ShortPair;
    Pair<int> IntPair;
};
