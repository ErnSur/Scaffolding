using System;

namespace QuickEye.Scaffolding
{
    [Serializable]
    public class FieldStyles : FieldDictionary<CaseStyle> { }

    [Serializable]
    public class FieldPrefixes : FieldDictionary<string> { }

    [Serializable]
    public class FieldDictionary<T>
    {
        public T @public;
        public T @private;
        public T @protected;
        public T @internal;
        public T protectedInternal;
        public T privateProtected;

        public T this[AccessModifier m]
        {
            get
            {
                switch (m)
                {
                    case AccessModifier.Public: return @public;
                    case AccessModifier.Private: return @private;
                    case AccessModifier.Protected: return @protected;
                    case AccessModifier.Internal: return @internal;
                    case AccessModifier.ProtectedInternal: return protectedInternal;
                    case AccessModifier.PrivateProtected: return privateProtected;
                    default: throw new NotImplementedException();
                }
            }
            set
            {
                switch (m)
                {
                    case AccessModifier.Public:
                        @public = value;
                        break;
                    case AccessModifier.Private:
                        @private = value;
                        break;
                    case AccessModifier.Protected:
                        @protected = value;
                        break;
                    case AccessModifier.Internal:
                        @internal = value;
                        break;
                    case AccessModifier.ProtectedInternal:
                        protectedInternal = value;
                        break;
                    case AccessModifier.PrivateProtected:
                        privateProtected = value;
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}