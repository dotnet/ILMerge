#if UseSingularityPDB

///////////////////////////////////////////////////////////////////////////////
//
//  Microsoft Research Singularity PDB Info Library
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File:   CvInfo.cs
//
//  Generic CodeView information definitions
//
//  Structures, constants, etc. for accessing and interpreting
//  CodeView information.
//
//  The master copy of this file resides in the langapi project (in C++).
//  All Microsoft projects are required to use the master copy without
//  modification.  Modification of the master version or a copy
//  without consultation with all parties concerned is extremely
//  risky.
//
//  When this file is modified, the corresponding documentation file
//  omfdeb.doc in the langapi project must be updated.
//
//  This is a read-only copy of the C++ file converted to C#.
//
using System;

namespace Microsoft.Singularity.PdbInfo.CodeView
{
    public struct FLOAT10
    {
        public byte Data_0;
        public byte Data_1;
        public byte Data_2;
        public byte Data_3;
        public byte Data_4;
        public byte Data_5;
        public byte Data_6;
        public byte Data_7;
        public byte Data_8;
        public byte Data_9;
    };

    public enum CV_SIGNATURE {
        C6          = 0,    // Actual signature is >64K
        C7          = 1,    // First explicit signature
        C11         = 2,    // C11 (vc5.x) 32-bit types
        C13         = 4,    // C13 (vc7.x) zero terminated names
        RESERVERD   = 5,    // All signatures from 5 to 64K are reserved
    };

    //  CodeView Symbol and Type OMF type information is broken up into two
    //  ranges.  Type indices less than 0x1000 describe type information
    //  that is frequently used.  Type indices above 0x1000 are used to
    //  describe more complex features such as functions, arrays and
    //  structures.
    //

    //  Primitive types have predefined meaning that is encoded in the
    //  values of the various bit fields in the value.
    //
    //  A CodeView primitive type is defined as:
    //
    //  1 1
    //  1 089  7654  3  210
    //  r mode type  r  sub
    //
    //  Where
    //      mode is the pointer mode
    //      type is a type indicator
    //      sub  is a subtype enumeration
    //      r    is a reserved field
    //
    //  See Microsoft Symbol and Type OMF (Version 4.0) for more
    //  information.
    //

    //  pointer mode enumeration values

    public enum CV_prmode {
        CV_TM_DIRECT    = 0,        // mode is not a pointer
        CV_TM_NPTR32    = 4,        // mode is a 32 bit near pointer
        CV_TM_NPTR64    = 6,        // mode is a 64 bit near pointer
        CV_TM_NPTR128   = 7,        // mode is a 128 bit near pointer
    };

    //  type enumeration values

    public enum CV_type {
        CV_SPECIAL      = 0x00,     // special type size values
        CV_SIGNED       = 0x01,     // signed integral size values
        CV_UNSIGNED     = 0x02,     // unsigned integral size values
        CV_BOOLEAN      = 0x03,     // Boolean size values
        CV_REAL         = 0x04,     // real number size values
        CV_COMPLEX      = 0x05,     // complex number size values
        CV_SPECIAL2     = 0x06,     // second set of special types
        CV_INT          = 0x07,     // integral (int) values
        CV_CVRESERVED   = 0x0f,
    };

    //  subtype enumeration values for CV_SPECIAL

    public enum CV_special {
        CV_SP_NOTYPE    = 0x00,
        CV_SP_ABS       = 0x01,
        CV_SP_SEGMENT   = 0x02,
        CV_SP_VOID      = 0x03,
        CV_SP_CURRENCY  = 0x04,
        CV_SP_NBASICSTR = 0x05,
        CV_SP_FBASICSTR = 0x06,
        CV_SP_NOTTRANS  = 0x07,
        CV_SP_HRESULT   = 0x08,
    };

    //  subtype enumeration values for CV_SPECIAL2

    public enum CV_special2 {
        CV_S2_BIT       = 0x00,
        CV_S2_PASCHAR   = 0x01,     // Pascal CHAR
    };

    //  subtype enumeration values for CV_SIGNED, CV_UNSIGNED and CV_BOOLEAN

    public enum CV_integral {
        CV_IN_1BYTE     = 0x00,
        CV_IN_2BYTE     = 0x01,
        CV_IN_4BYTE     = 0x02,
        CV_IN_8BYTE     = 0x03,
        CV_IN_16BYTE    = 0x04,
    };

    //  subtype enumeration values for CV_REAL and CV_COMPLEX

    public enum CV_real {
        CV_RC_REAL32    = 0x00,
        CV_RC_REAL64    = 0x01,
        CV_RC_REAL80    = 0x02,
        CV_RC_REAL128   = 0x03,
    };

    //  subtype enumeration values for CV_INT (really int)

    public enum CV_int {
        CV_RI_CHAR      = 0x00,
        CV_RI_INT1      = 0x00,
        CV_RI_WCHAR     = 0x01,
        CV_RI_UINT1     = 0x01,
        CV_RI_INT2      = 0x02,
        CV_RI_UINT2     = 0x03,
        CV_RI_INT4      = 0x04,
        CV_RI_UINT4     = 0x05,
        CV_RI_INT8      = 0x06,
        CV_RI_UINT8     = 0x07,
        CV_RI_INT16     = 0x08,
        CV_RI_UINT16    = 0x09,
    };

    public struct CV_PRIMITIVE_TYPE
    {
        const uint CV_MMASK     = 0x700;       // mode mask
        const uint CV_TMASK     = 0x0f0;       // type mask
        const uint CV_SMASK     = 0x00f;       // subtype mask

        const int CV_MSHIFT     = 8;           // primitive mode right shift count
        const int CV_TSHIFT     = 4;           // primitive type right shift count
        const int CV_SSHIFT     = 0;           // primitive subtype right shift count

        // function to extract primitive mode, type and size

        public static CV_prmode CV_MODE(TYPE_ENUM typ)
        {
            return (CV_prmode)((((uint)typ) & CV_MMASK) >> CV_MSHIFT);
        }

        public static CV_type CV_TYPE(TYPE_ENUM typ)
        {
            return (CV_type)((((uint)typ) & CV_TMASK) >> CV_TSHIFT);
        }

        public static uint CV_SUBT(TYPE_ENUM typ)
        {
            return ((((uint)typ) & CV_SMASK) >> CV_SSHIFT);
        }

        // functions to check the type of a primitive

        public static bool CV_TYP_IS_DIRECT(TYPE_ENUM typ)
        {
            return (CV_MODE(typ) == CV_prmode.CV_TM_DIRECT);
        }

        public static bool CV_TYP_IS_PTR(TYPE_ENUM typ)
        {
            return (CV_MODE(typ) != CV_prmode.CV_TM_DIRECT);
        }

        public static bool CV_TYP_IS_SIGNED(TYPE_ENUM typ)
        {
            return
                (((CV_TYPE(typ) == CV_type.CV_SIGNED) && CV_TYP_IS_DIRECT(typ)) ||
                 (typ == TYPE_ENUM.T_INT1)  ||
                 (typ == TYPE_ENUM.T_INT2)  ||
                 (typ == TYPE_ENUM.T_INT4)  ||
                 (typ == TYPE_ENUM.T_INT8)  ||
                 (typ == TYPE_ENUM.T_INT16) ||
                 (typ == TYPE_ENUM.T_RCHAR));
        }

        public static bool CV_TYP_IS_UNSIGNED(TYPE_ENUM typ)
        {
            return (((CV_TYPE(typ) == CV_type.CV_UNSIGNED) && CV_TYP_IS_DIRECT(typ)) ||
                    (typ == TYPE_ENUM.T_UINT1) ||
                    (typ == TYPE_ENUM.T_UINT2) ||
                    (typ == TYPE_ENUM.T_UINT4) ||
                    (typ == TYPE_ENUM.T_UINT8) ||
                    (typ == TYPE_ENUM.T_UINT16));
        }

        public static bool CV_TYP_IS_REAL(TYPE_ENUM typ)
        {
            return ((CV_TYPE(typ) == CV_type.CV_REAL)  && CV_TYP_IS_DIRECT(typ));
        }

        const uint CV_FIRST_NONPRIM = 0x1000;

        public static bool CV_IS_PRIMITIVE(TYPE_ENUM typ)
        {
            return ((uint)(typ) < CV_FIRST_NONPRIM);
        }

        public static bool CV_TYP_IS_COMPLEX(TYPE_ENUM typ)
        {
            return ((CV_TYPE(typ) == CV_type.CV_COMPLEX) && CV_TYP_IS_DIRECT(typ));
        }

        public static bool CV_IS_INTERNAL_PTR(TYPE_ENUM typ)
        {
            return (CV_IS_PRIMITIVE(typ) &&
                    CV_TYPE(typ) == CV_type.CV_CVRESERVED &&
                    CV_TYP_IS_PTR(typ));
        }
    }

    // selected values for type_index - for a more complete definition, see
    // Microsoft Symbol and Type OMF document

    //  Special Types

    public enum TYPE_ENUM {
        //  Special Types

        T_NOTYPE        = 0x0000,   // uncharacterized type (no type)
        T_ABS           = 0x0001,   // absolute symbol
        T_SEGMENT       = 0x0002,   // segment type
        T_VOID          = 0x0003,   // void
        T_HRESULT       = 0x0008,   // OLE/COM HRESULT
        T_32PHRESULT    = 0x0408,   // OLE/COM HRESULT __ptr32//
        T_64PHRESULT    = 0x0608,   // OLE/COM HRESULT __ptr64//
        T_PVOID         = 0x0103,   // near pointer to void
        T_PFVOID        = 0x0203,   // far pointer to void
        T_PHVOID        = 0x0303,   // huge pointer to void
        T_32PVOID       = 0x0403,   // 32 bit pointer to void
        T_64PVOID       = 0x0603,   // 64 bit pointer to void
        T_CURRENCY      = 0x0004,   // BASIC 8 byte currency value
        T_NOTTRANS      = 0x0007,   // type not translated by cvpack
        T_BIT           = 0x0060,   // bit
        T_PASCHAR       = 0x0061,   // Pascal CHAR

        //  Character types

        T_CHAR          = 0x0010,   // 8 bit signed
        T_32PCHAR       = 0x0410,   // 32 bit pointer to 8 bit signed
        T_64PCHAR       = 0x0610,   // 64 bit pointer to 8 bit signed

        T_UCHAR         = 0x0020,   // 8 bit unsigned
        T_32PUCHAR      = 0x0420,   // 32 bit pointer to 8 bit unsigned
        T_64PUCHAR      = 0x0620,   // 64 bit pointer to 8 bit unsigned

        //  really a character types

        T_RCHAR         = 0x0070,   // really a char
        T_32PRCHAR      = 0x0470,   // 32 bit pointer to a real char
        T_64PRCHAR      = 0x0670,   // 64 bit pointer to a real char

        //  really a wide character types

        T_WCHAR         = 0x0071,   // wide char
        T_32PWCHAR      = 0x0471,   // 32 bit pointer to a wide char
        T_64PWCHAR      = 0x0671,   // 64 bit pointer to a wide char

        //  8 bit int types

        T_INT1          = 0x0068,   // 8 bit signed int
        T_32PINT1       = 0x0468,   // 32 bit pointer to 8 bit signed int
        T_64PINT1       = 0x0668,   // 64 bit pointer to 8 bit signed int

        T_UINT1         = 0x0069,   // 8 bit unsigned int
        T_32PUINT1      = 0x0469,   // 32 bit pointer to 8 bit unsigned int
        T_64PUINT1      = 0x0669,   // 64 bit pointer to 8 bit unsigned int

        //  16 bit short types

        T_SHORT         = 0x0011,   // 16 bit signed
        T_32PSHORT      = 0x0411,   // 32 bit pointer to 16 bit signed
        T_64PSHORT      = 0x0611,   // 64 bit pointer to 16 bit signed

        T_USHORT        = 0x0021,   // 16 bit unsigned
        T_32PUSHORT     = 0x0421,   // 32 bit pointer to 16 bit unsigned
        T_64PUSHORT     = 0x0621,   // 64 bit pointer to 16 bit unsigned

        //  16 bit int types

        T_INT2          = 0x0072,   // 16 bit signed int
        T_32PINT2       = 0x0472,   // 32 bit pointer to 16 bit signed int
        T_64PINT2       = 0x0672,   // 64 bit pointer to 16 bit signed int

        T_UINT2         = 0x0073,   // 16 bit unsigned int
        T_32PUINT2      = 0x0473,   // 32 bit pointer to 16 bit unsigned int
        T_64PUINT2      = 0x0673,   // 64 bit pointer to 16 bit unsigned int

        //  32 bit long types

        T_LONG          = 0x0012,   // 32 bit signed
        T_ULONG         = 0x0022,   // 32 bit unsigned
        T_32PLONG       = 0x0412,   // 32 bit pointer to 32 bit signed
        T_32PULONG      = 0x0422,   // 32 bit pointer to 32 bit unsigned
        T_64PLONG       = 0x0612,   // 64 bit pointer to 32 bit signed
        T_64PULONG      = 0x0622,   // 64 bit pointer to 32 bit unsigned

        //  32 bit int types

        T_INT4          = 0x0074,   // 32 bit signed int
        T_32PINT4       = 0x0474,   // 32 bit pointer to 32 bit signed int
        T_64PINT4       = 0x0674,   // 64 bit pointer to 32 bit signed int

        T_UINT4         = 0x0075,   // 32 bit unsigned int
        T_32PUINT4      = 0x0475,   // 32 bit pointer to 32 bit unsigned int
        T_64PUINT4      = 0x0675,   // 64 bit pointer to 32 bit unsigned int

        //  64 bit quad types

        T_QUAD          = 0x0013,   // 64 bit signed
        T_32PQUAD       = 0x0413,   // 32 bit pointer to 64 bit signed
        T_64PQUAD       = 0x0613,   // 64 bit pointer to 64 bit signed

        T_UQUAD         = 0x0023,   // 64 bit unsigned
        T_32PUQUAD      = 0x0423,   // 32 bit pointer to 64 bit unsigned
        T_64PUQUAD      = 0x0623,   // 64 bit pointer to 64 bit unsigned

        //  64 bit int types

        T_INT8          = 0x0076,   // 64 bit signed int
        T_32PINT8       = 0x0476,   // 32 bit pointer to 64 bit signed int
        T_64PINT8       = 0x0676,   // 64 bit pointer to 64 bit signed int

        T_UINT8         = 0x0077,   // 64 bit unsigned int
        T_32PUINT8      = 0x0477,   // 32 bit pointer to 64 bit unsigned int
        T_64PUINT8      = 0x0677,   // 64 bit pointer to 64 bit unsigned int

        //  128 bit octet types

        T_OCT           = 0x0014,   // 128 bit signed
        T_32POCT        = 0x0414,   // 32 bit pointer to 128 bit signed
        T_64POCT        = 0x0614,   // 64 bit pointer to 128 bit signed

        T_UOCT          = 0x0024,   // 128 bit unsigned
        T_32PUOCT       = 0x0424,   // 32 bit pointer to 128 bit unsigned
        T_64PUOCT       = 0x0624,   // 64 bit pointer to 128 bit unsigned

        //  128 bit int types

        T_INT16         = 0x0078,   // 128 bit signed int
        T_32PINT16      = 0x0478,   // 32 bit pointer to 128 bit signed int
        T_64PINT16      = 0x0678,   // 64 bit pointer to 128 bit signed int

        T_UINT16        = 0x0079,   // 128 bit unsigned int
        T_32PUINT16     = 0x0479,   // 32 bit pointer to 128 bit unsigned int
        T_64PUINT16     = 0x0679,   // 64 bit pointer to 128 bit unsigned int

        //  32 bit real types

        T_REAL32        = 0x0040,   // 32 bit real
        T_32PREAL32     = 0x0440,   // 32 bit pointer to 32 bit real
        T_64PREAL32     = 0x0640,   // 64 bit pointer to 32 bit real

        //  64 bit real types

        T_REAL64        = 0x0041,   // 64 bit real
        T_32PREAL64     = 0x0441,   // 32 bit pointer to 64 bit real
        T_64PREAL64     = 0x0641,   // 64 bit pointer to 64 bit real

        //  80 bit real types

        T_REAL80        = 0x0042,   // 80 bit real
        T_32PREAL80     = 0x0442,   // 32 bit pointer to 80 bit real
        T_64PREAL80     = 0x0642,   // 64 bit pointer to 80 bit real

        //  128 bit real types

        T_REAL128       = 0x0043,   // 128 bit real
        T_32PREAL128    = 0x0443,   // 32 bit pointer to 128 bit real
        T_64PREAL128    = 0x0643,   // 64 bit pointer to 128 bit real

        //  32 bit complex types

        T_CPLX32        = 0x0050,   // 32 bit complex
        T_32PCPLX32     = 0x0450,   // 32 bit pointer to 32 bit complex
        T_64PCPLX32     = 0x0650,   // 64 bit pointer to 32 bit complex

        //  64 bit complex types

        T_CPLX64        = 0x0051,   // 64 bit complex
        T_32PCPLX64     = 0x0451,   // 32 bit pointer to 64 bit complex
        T_64PCPLX64     = 0x0651,   // 64 bit pointer to 64 bit complex

        //  80 bit complex types

        T_CPLX80        = 0x0052,   // 80 bit complex
        T_32PCPLX80     = 0x0452,   // 32 bit pointer to 80 bit complex
        T_64PCPLX80     = 0x0652,   // 64 bit pointer to 80 bit complex

        //  128 bit complex types

        T_CPLX128       = 0x0053,   // 128 bit complex
        T_32PCPLX128    = 0x0453,   // 32 bit pointer to 128 bit complex
        T_64PCPLX128    = 0x0653,   // 64 bit pointer to 128 bit complex

        //  boolean types

        T_BOOL08        = 0x0030,   // 8 bit boolean
        T_32PBOOL08     = 0x0430,   // 32 bit pointer to 8 bit boolean
        T_64PBOOL08     = 0x0630,   // 64 bit pointer to 8 bit boolean

        T_BOOL16        = 0x0031,   // 16 bit boolean
        T_32PBOOL16     = 0x0431,   // 32 bit pointer to 18 bit boolean
        T_64PBOOL16     = 0x0631,   // 64 bit pointer to 18 bit boolean

        T_BOOL32        = 0x0032,   // 32 bit boolean
        T_32PBOOL32     = 0x0432,   // 32 bit pointer to 32 bit boolean
        T_64PBOOL32     = 0x0632,   // 64 bit pointer to 32 bit boolean

        T_BOOL64        = 0x0033,   // 64 bit boolean
        T_32PBOOL64     = 0x0433,   // 32 bit pointer to 64 bit boolean
        T_64PBOOL64     = 0x0633,   // 64 bit pointer to 64 bit boolean
    };

    //  No leaf index can have a value of 0x0000.  The leaf indices are
    //  separated into ranges depending upon the use of the type record.
    //  The second range is for the type records that are directly referenced
    //  in symbols. The first range is for type records that are not
    //  referenced by symbols but instead are referenced by other type
    //  records.  All type records must have a starting leaf index in these
    //  first two ranges.  The third range of leaf indices are used to build
    //  up complex lists such as the field list of a class type record.  No
    //  type record can begin with one of the leaf indices. The fourth ranges
    //  of type indices are used to represent numeric data in a symbol or
    //  type record. These leaf indices are greater than 0x8000.  At the
    //  point that type or symbol processor is expecting a numeric field, the
    //  next two bytes in the type record are examined.  If the value is less
    //  than 0x8000, then the two bytes contain the numeric value.  If the
    //  value is greater than 0x8000, then the data follows the leaf index in
    //  a format specified by the leaf index. The final range of leaf indices
    //  are used to force alignment of subfields within a complex type record..
    //

    public enum LEAF {
        // leaf indices starting records but referenced from symbol records

        LF_VTSHAPE          = 0x000a,
        LF_COBOL1           = 0x000c,
        LF_LABEL            = 0x000e,
        LF_NULL             = 0x000f,
        LF_NOTTRAN          = 0x0010,
        LF_ENDPRECOMP       = 0x0014,       // not referenced from symbol
        LF_TYPESERVER_ST    = 0x0016,       // not referenced from symbol

        // leaf indices starting records but referenced only from type records

        LF_LIST             = 0x0203,
        LF_REFSYM           = 0x020c,

        LF_ENUMERATE_ST     = 0x0403,

        // 32-bit type index versions of leaves, all have the 0x1000 bit set
        //
        LF_TI16_MAX         = 0x1000,

        LF_MODIFIER         = 0x1001,
        LF_POINTER          = 0x1002,
        LF_ARRAY_ST         = 0x1003,
        LF_CLASS_ST         = 0x1004,
        LF_STRUCTURE_ST     = 0x1005,
        LF_UNION_ST         = 0x1006,
        LF_ENUM_ST          = 0x1007,
        LF_PROCEDURE        = 0x1008,
        LF_MFUNCTION        = 0x1009,
        LF_COBOL0           = 0x100a,
        LF_BARRAY           = 0x100b,
        LF_DIMARRAY_ST      = 0x100c,
        LF_VFTPATH          = 0x100d,
        LF_PRECOMP_ST       = 0x100e,       // not referenced from symbol
        LF_OEM              = 0x100f,       // oem definable type string
        LF_ALIAS_ST         = 0x1010,       // alias (typedef) type
        LF_OEM2             = 0x1011,       // oem definable type string

        // leaf indices starting records but referenced only from type records

        LF_SKIP             = 0x1200,
        LF_ARGLIST          = 0x1201,
        LF_DEFARG_ST        = 0x1202,
        LF_FIELDLIST        = 0x1203,
        LF_DERIVED          = 0x1204,
        LF_BITFIELD         = 0x1205,
        LF_METHODLIST       = 0x1206,
        LF_DIMCONU          = 0x1207,
        LF_DIMCONLU         = 0x1208,
        LF_DIMVARU          = 0x1209,
        LF_DIMVARLU         = 0x120a,

        LF_BCLASS           = 0x1400,
        LF_VBCLASS          = 0x1401,
        LF_IVBCLASS         = 0x1402,
        LF_FRIENDFCN_ST     = 0x1403,
        LF_INDEX            = 0x1404,
        LF_MEMBER_ST        = 0x1405,
        LF_STMEMBER_ST      = 0x1406,
        LF_METHOD_ST        = 0x1407,
        LF_NESTTYPE_ST      = 0x1408,
        LF_VFUNCTAB         = 0x1409,
        LF_FRIENDCLS        = 0x140a,
        LF_ONEMETHOD_ST     = 0x140b,
        LF_VFUNCOFF         = 0x140c,
        LF_NESTTYPEEX_ST    = 0x140d,
        LF_MEMBERMODIFY_ST  = 0x140e,
        LF_MANAGED_ST       = 0x140f,

        // Types w/ SZ names

        LF_ST_MAX           = 0x1500,

        LF_TYPESERVER       = 0x1501,       // not referenced from symbol
        LF_ENUMERATE        = 0x1502,
        LF_ARRAY            = 0x1503,
        LF_CLASS            = 0x1504,
        LF_STRUCTURE        = 0x1505,
        LF_UNION            = 0x1506,
        LF_ENUM             = 0x1507,
        LF_DIMARRAY         = 0x1508,
        LF_PRECOMP          = 0x1509,       // not referenced from symbol
        LF_ALIAS            = 0x150a,       // alias (typedef) type
        LF_DEFARG           = 0x150b,
        LF_FRIENDFCN        = 0x150c,
        LF_MEMBER           = 0x150d,
        LF_STMEMBER         = 0x150e,
        LF_METHOD           = 0x150f,
        LF_NESTTYPE         = 0x1510,
        LF_ONEMETHOD        = 0x1511,
        LF_NESTTYPEEX       = 0x1512,
        LF_MEMBERMODIFY     = 0x1513,
        LF_MANAGED          = 0x1514,
        LF_TYPESERVER2      = 0x1515,

        LF_NUMERIC          = 0x8000,
        LF_CHAR             = 0x8000,
        LF_SHORT            = 0x8001,
        LF_USHORT           = 0x8002,
        LF_LONG             = 0x8003,
        LF_ULONG            = 0x8004,
        LF_REAL32           = 0x8005,
        LF_REAL64           = 0x8006,
        LF_REAL80           = 0x8007,
        LF_REAL128          = 0x8008,
        LF_QUADWORD         = 0x8009,
        LF_UQUADWORD        = 0x800a,
        LF_COMPLEX32        = 0x800c,
        LF_COMPLEX64        = 0x800d,
        LF_COMPLEX80        = 0x800e,
        LF_COMPLEX128       = 0x800f,
        LF_VARSTRING        = 0x8010,

        LF_OCTWORD          = 0x8017,
        LF_UOCTWORD         = 0x8018,

        LF_DECIMAL          = 0x8019,
        LF_DATE             = 0x801a,
        LF_UTF8STRING       = 0x801b,

        LF_PAD0             = 0xf0,
        LF_PAD1             = 0xf1,
        LF_PAD2             = 0xf2,
        LF_PAD3             = 0xf3,
        LF_PAD4             = 0xf4,
        LF_PAD5             = 0xf5,
        LF_PAD6             = 0xf6,
        LF_PAD7             = 0xf7,
        LF_PAD8             = 0xf8,
        LF_PAD9             = 0xf9,
        LF_PAD10            = 0xfa,
        LF_PAD11            = 0xfb,
        LF_PAD12            = 0xfc,
        LF_PAD13            = 0xfd,
        LF_PAD14            = 0xfe,
        LF_PAD15            = 0xff,

    };

    // end of leaf indices

    //  Type enum for pointer records
    //  Pointers can be one of the following types

    public enum CV_ptrtype {
        CV_PTR_BASE_SEG     = 0x03, // based on segment
        CV_PTR_BASE_VAL     = 0x04, // based on value of base
        CV_PTR_BASE_SEGVAL  = 0x05, // based on segment value of base
        CV_PTR_BASE_ADDR    = 0x06, // based on address of base
        CV_PTR_BASE_SEGADDR = 0x07, // based on segment address of base
        CV_PTR_BASE_TYPE    = 0x08, // based on type
        CV_PTR_BASE_SELF    = 0x09, // based on self
        CV_PTR_NEAR32       = 0x0a, // 32 bit pointer
        CV_PTR_64           = 0x0c, // 64 bit pointer
        CV_PTR_UNUSEDPTR    = 0x0d  // first unused pointer type
    };

    //  Mode enum for pointers
    //  Pointers can have one of the following modes

    public enum CV_ptrmode {
        CV_PTR_MODE_PTR     = 0x00, // "normal" pointer
        CV_PTR_MODE_REF     = 0x01, // reference
        CV_PTR_MODE_PMEM    = 0x02, // pointer to data member
        CV_PTR_MODE_PMFUNC  = 0x03, // pointer to member function
        CV_PTR_MODE_RESERVED= 0x04  // first unused pointer mode
    };

    //  enumeration for pointer-to-member types

    public enum CV_pmtype {
        CV_PMTYPE_Undef     = 0x00, // not specified (pre VC8)
        CV_PMTYPE_D_Single  = 0x01, // member data, single inheritance
        CV_PMTYPE_D_Multiple= 0x02, // member data, multiple inheritance
        CV_PMTYPE_D_Virtual = 0x03, // member data, virtual inheritance
        CV_PMTYPE_D_General = 0x04, // member data, most general
        CV_PMTYPE_F_Single  = 0x05, // member function, single inheritance
        CV_PMTYPE_F_Multiple= 0x06, // member function, multiple inheritance
        CV_PMTYPE_F_Virtual = 0x07, // member function, virtual inheritance
        CV_PMTYPE_F_General = 0x08, // member function, most general
    };

    //  enumeration for method properties

    public enum CV_methodprop {
        CV_MTvanilla        = 0x00,
        CV_MTvirtual        = 0x01,
        CV_MTstatic         = 0x02,
        CV_MTfriend         = 0x03,
        CV_MTintro          = 0x04,
        CV_MTpurevirt       = 0x05,
        CV_MTpureintro      = 0x06
    };

    //  enumeration for virtual shape table entries

    public enum CV_VTS_desc {
        CV_VTS_near         = 0x00,
        CV_VTS_far          = 0x01,
        CV_VTS_thin         = 0x02,
        CV_VTS_outer        = 0x03,
        CV_VTS_meta         = 0x04,
        CV_VTS_near32       = 0x05,
        CV_VTS_far32        = 0x06,
        CV_VTS_unused       = 0x07
    };

    //  enumeration for LF_LABEL address modes

    public enum CV_LABEL_TYPE {
        CV_LABEL_NEAR = 0,       // near return
        CV_LABEL_FAR  = 4        // far return
    };

    //  enumeration for LF_MODIFIER values

    [Flags]
    public enum CV_modifier : ushort {
        MOD_const       = 0x0001,
        MOD_volatile    = 0x0002,
        MOD_unaligned   = 0x0004,
    };

    //  bit field structure describing class/struct/union/enum properties

    [Flags]
    public enum CV_prop : ushort {
        packed          = 0x0001,   // true if structure is packed
        ctor            = 0x0002,   // true if constructors or destructors present
        ovlops          = 0x0004,   // true if overloaded operators present
        isnested        = 0x0008,   // true if this is a nested class
        cnested         = 0x0010,   // true if this class contains nested types
        opassign        = 0x0020,   // true if overloaded assignment (=)
        opcast          = 0x0040,   // true if casting methods
        fwdref          = 0x0080,   // true if forward reference (incomplete defn)
        scoped          = 0x0100,   // scoped definition
    }

    //  class field attribute

    [Flags]
    public enum CV_fldattr {
        access          = 0x0003,   // access protection CV_access_t
        mprop           = 0x001c,   // method properties CV_methodprop_t
        pseudo          = 0x0020,   // compiler generated fcn and does not exist
        noinherit       = 0x0040,   // true if class cannot be inherited
        noconstruct     = 0x0080,   // true if class cannot be constructed
        compgenx        = 0x0100,   // compiler generated fcn and does exist
    }

    //  Structures to access to the type records

    public struct TYPTYPE {
        public ushort   len;
        public ushort   leaf;
        // byte data[];

        //  char *NextType (char * pType) {
        //  return (pType + ((TYPTYPE *)pType)->len + sizeof(ushort));
        //  }
    };          // general types record

    //  memory representation of pointer to member.  These representations are
    //  indexed by the enumeration above in the LF_POINTER record

    //  representation of a 32 bit pointer to data for a class with
    //  or without virtual functions and no virtual bases

    public struct CV_PDMR32_NVVFCN {
        public int      mdisp;      // displacement to data (NULL = 0x80000000)
    };

    //  representation of a 32 bit pointer to data for a class
    //  with virtual bases

    public struct CV_PDMR32_VBASE {
        public int      mdisp;      // displacement to data
        public int      pdisp;      // this pointer displacement
        public int      vdisp;      // vbase table displacement
        // NULL = (,,0xffffffff)
    };

    //  representation of a 32 bit pointer to member function for a
    //  class with no virtual bases and a single address point

    public struct CV_PMFR32_NVSA {
        public uint     off;        // near address of function (NULL = 0L)
    };

    //  representation of a 32 bit pointer to member function for a
    //  class with no virtual bases and multiple address points

    public struct CV_PMFR32_NVMA {
        public uint     off;        // near address of function (NULL = 0L,x)
        public int      disp;
    };

    //  representation of a 32 bit pointer to member function for a
    //  class with virtual bases

    public struct CV_PMFR32_VBASE {
        public uint     off;        // near address of function (NULL = 0L,x,x,x)
        public int      mdisp;      // displacement to data
        public int      pdisp;      // this pointer displacement
        public int      vdisp;      // vbase table displacement
    };

    //////////////////////////////////////////////////////////////////////////////
    //
    //  The following type records are basically variant records of the
    //  above structure.  The "ushort leaf" of the above structure and
    //  the "ushort leaf" of the following type definitions are the same
    //  symbol.
    //

    //  Notes on alignment
    //  Alignment of the fields in most of the type records is done on the
    //  basis of the TYPTYPE record base.  That is why in most of the lf*
    //  records that the type is located on what appears to
    //  be a offset mod 4 == 2 boundary.  The exception to this rule are those
    //  records that are in a list (lfFieldList, lfMethodList), which are
    //  aligned to their own bases since they don't have the length field
    //

    //  Type record for LF_MODIFIER

    public struct LeafModifier {
        // public ushort leaf;      // LF_MODIFIER [TYPTYPE]
        public uint     type;       // (type index) modified type
        public CV_modifier attr;    // modifier attribute modifier_t
    };

    //  type record for LF_POINTER

    [Flags]
    public enum LeafPointerAttr : uint {
        ptrtype     = 0x0000001f,   // ordinal specifying pointer type (CV_ptrtype)
        ptrmode     = 0x000000e0,   // ordinal specifying pointer mode (CV_ptrmode)
        isflat32    = 0x00000100,   // true if 0:32 pointer
        isvolatile  = 0x00000200,   // TRUE if volatile pointer
        isconst     = 0x00000400,   // TRUE if const pointer
        isunaligned = 0x00000800,   // TRUE if unaligned pointer
        isrestrict  = 0x00001000,   // TRUE if restricted pointer (allow agressive opts)
    };

    public struct LeafPointer {
        public struct LeafPointerBody {
            // public ushort leaf;  // LF_POINTER [TYPTYPE]
            public uint     utype;  // (type index) type index of the underlying type
            public LeafPointerAttr attr;
        };
#if false
        union {
            public struct {
                uint    pmclass;    // (type index) index of containing class for pointer to member
                ushort  pmenum;     // enumeration specifying pm format (CV_pmtype)
            };
            ushort  bseg;           // base segment if PTR_BASE_SEG
            byte[]  Sym;            // copy of base symbol record (including length)
            public struct  {
                uint    index;      // (type index) type index if CV_PTR_BASE_TYPE
                string  name;       // name of base type
            } btype;
        } pbase;
#endif
    }

    //  type record for LF_ARRAY

    public struct LeafArray {
        // public ushort leaf;      // LF_ARRAY [TYPTYPE]
        public uint     elemtype;   // (type index) type index of element type
        public uint     idxtype;    // (type index) type index of indexing type
        public byte[]   data;       // variable length data specifying size in bytes
        public string   name;
    };

    //  type record for LF_CLASS, LF_STRUCTURE

    public struct LeafClass {
        // public ushort leaf;      // LF_CLASS, LF_STRUCT [TYPTYPE]
        public ushort   count;      // count of number of elements in class
        public ushort   property;   // (CV_prop_t) property attribute field (prop_t)
        public uint     field;      // (type index) type index of LF_FIELD descriptor list
        public uint     derived;    // (type index) type index of derived from list if not zero
        public uint     vshape;     // (type index) type index of vshape table for this class
        public byte[]   data;       // data describing length of structure in bytes
        public string   name;
    };

    //  type record for LF_UNION

    public struct LeafUnion {
        // public ushort leaf;      // LF_UNION [TYPTYPE]
        public ushort   count;      // count of number of elements in class
        public ushort   property;   // (CV_prop_t) property attribute field
        public uint     field;      // (type index) type index of LF_FIELD descriptor list
        public byte[]   data;       // variable length data describing length of
        public string   name;
    };

    //  type record for LF_ALIAS

    public struct LeafAlias {
        // public ushort leaf;      // LF_ALIAS [TYPTYPE]
        public uint     utype;      // (type index) underlying type
        public string   name;       // alias name
    };

    //  type record for LF_MANAGED

    public struct LeafManaged {
        // public ushort leaf;      // LF_MANAGED [TYPTYPE]
        public string   name;       // utf8, zero terminated managed type name
    };

    //  type record for LF_ENUM

    public struct LeafEnum {
        // public ushort leaf;      // LF_ENUM [TYPTYPE]
        public ushort   count;      // count of number of elements in class
        public ushort   property;   // (CV_propt_t) property attribute field
        public uint     utype;      // (type index) underlying type of the enum
        public uint     field;      // (type index) type index of LF_FIELD descriptor list
        public string   name;       // length prefixed name of enum
    };

    //  Type record for LF_PROCEDURE

    public struct LeafProc {
        // public ushort leaf;      // LF_PROCEDURE [TYPTYPE]
        public uint     rvtype;     // (type index) type index of return value
        public byte     calltype;   // calling convention (CV_call_t)
        public byte     reserved;   // reserved for future use
        public ushort   parmcount;  // number of parameters
        public uint     arglist;    // (type index) type index of argument list
    };

    //  Type record for member function

    public struct LeafMFunc {
        // public ushort leaf;      // LF_MFUNCTION [TYPTYPE]
        public uint     rvtype;     // (type index) type index of return value
        public uint     classtype;  // (type index) type index of containing class
        public uint     thistype;   // (type index) type index of this pointer (model specific)
        public byte     calltype;   // calling convention (call_t)
        public byte     reserved;   // reserved for future use
        public ushort   parmcount;  // number of parameters
        public uint     arglist;    // (type index) type index of argument list
        public int      thisadjust; // this adjuster (long because pad required anyway)
    };

    //  type record for virtual function table shape

    public struct LeafVTShape {
        // public ushort leaf;      // LF_VTSHAPE [TYPTYPE]
        public ushort   count;      // number of entries in vfunctable
        public byte[]   desc;       // 4 bit (CV_VTS_desc) descriptors
    };

    //  type record for cobol0

    public struct LeafCobol0 {
        // public ushort leaf;      // LF_COBOL0 [TYPTYPE]
        public uint     type;       // (type index) parent type record index
        public byte[]   data;
    };

    //  type record for cobol1

    public struct LeafCobol1 {
        // public ushort leaf;      // LF_COBOL1 [TYPTYPE]
        public byte[]   data;
    };

    //  type record for basic array

    public struct LeafBArray {
        // public ushort leaf;      // LF_BARRAY [TYPTYPE]
        public uint     utype;      // (type index) type index of underlying type
    };

    //  type record for assembler labels

    public struct LeafLabel {
        // public ushort leaf;      // LF_LABEL [TYPTYPE]
        public ushort   mode;       // addressing mode of label
    };

    //  type record for dimensioned arrays

    public struct LeafDimArray {
        // public ushort leaf;      // LF_DIMARRAY [TYPTYPE]
        public uint     utype;      // (type index) underlying type of the array
        public uint     diminfo;    // (type index) dimension information
        public string   name;       // length prefixed name
    };

    //  type record describing path to virtual function table

    public struct LeafVFTPath {
        // public ushort leaf;      // LF_VFTPATH [TYPTYPE]
        public uint     count;      // count of number of bases in path
        public uint[]   bases;      // (type index) bases from root to leaf
    };

    //  type record describing inclusion of precompiled types

    public struct LeafPreComp {
        // public ushort leaf;      // LF_PRECOMP [TYPTYPE]
        public uint     start;      // starting type index included
        public uint     count;      // number of types in inclusion
        public uint     signature;  // signature
        public string   name;       // length prefixed name of included type file
    };

    //  type record describing end of precompiled types that can be
    //  included by another file

    public struct LeafEndPreComp {
        // public ushort leaf;      // LF_ENDPRECOMP [TYPTYPE]
        public uint     signature;  // signature
    };

    //  type record for OEM definable type strings

    public struct LeafOEM {
        // public ushort leaf;      // LF_OEM [TYPTYPE]
        public ushort   cvOEM;      // MS assigned OEM identified
        public ushort   recOEM;     // OEM assigned type identifier
        public uint     count;      // count of type indices to follow
        public uint[]   index;      // (type index) array of type indices followed
        // by OEM defined data
    };

    public enum OEM_ID {
        OEM_MS_FORTRAN90       = 0xF090,
        OEM_ODI                = 0x0010,
        OEM_THOMSON_SOFTWARE   = 0x5453,
        OEM_ODI_REC_BASELIST   = 0x0000,
    };

    public struct LeafOEM2 {
        // public ushort leaf;      // LF_OEM2 [TYPTYPE]
        public Guid     idOem;      // an oem ID (Guid)
        public uint     count;      // count of type indices to follow
        public uint[]   index;      // (type index) array of type indices followed
        // by OEM defined data
    };

    //  type record describing using of a type server

    public struct LeafTypeServer {
        // public ushort leaf;      // LF_TYPESERVER [TYPTYPE]
        public uint     signature;  // signature
        public uint     age;        // age of database used by this module
        public string   name;       // length prefixed name of PDB
    };

    //  type record describing using of a type server with v7 (GUID) signatures

    public struct LeafTypeServer2 {
        // public ushort leaf;      // LF_TYPESERVER2 [TYPTYPE]
        public Guid     sig70;      // guid signature
        public uint     age;        // age of database used by this module
        public string   name;       // length prefixed name of PDB
    };

    //  description of type records that can be referenced from
    //  type records referenced by symbols

    //  type record for skip record

    public struct LeafSkip {
        // public ushort leaf;      // LF_SKIP [TYPTYPE]
        public uint     type;       // (type index) next valid index
        public byte[]   data;       // pad data
    };

    //  argument list leaf

    public struct LeafArgList {
        // public ushort leaf;      // LF_ARGLIST [TYPTYPE]
        public uint     count;      // number of arguments
        public uint[]   arg;        // (type index) number of arguments
    };

    //  derived class list leaf

    public struct LeafDerived {
        // public ushort leaf;      // LF_DERIVED [TYPTYPE]
        public uint     count;      // number of arguments
        public uint[]   drvdcls;    // (type index) type indices of derived classes
    };

    //  leaf for default arguments

    public struct LeafDefArg {
        // public ushort leaf;      // LF_DEFARG [TYPTYPE]
        public uint     type;       // (type index) type of resulting expression
        public byte[]   expr;       // length prefixed expression string
    };

    //  list leaf
    //      This list should no longer be used because the utilities cannot
    //      verify the contents of the list without knowing what type of list
    //      it is.  New specific leaf indices should be used instead.

    public struct LeafList {
        // public ushort leaf;      // LF_LIST [TYPTYPE]
        public byte[]   data;       // data format specified by indexing type
    };

    //  field list leaf
    //  This is the header leaf for a complex list of class and structure
    //  subfields.

    public struct LeafFieldList {
        // public ushort leaf;      // LF_FIELDLIST [TYPTYPE]
        public char[]   data;       // field list sub lists
    };

    //  type record for non-static methods and friends in overloaded method list

    public struct mlMethod {
        public ushort   attr;       // (CV_fldattr_t) method attribute
        public ushort   pad0;       // internal padding, must be 0
        public uint     index;      // (type index) index to type record for procedure
        public uint[]   vbaseoff;   // offset in vfunctable if intro virtual
    };

    public struct LeafMethodList {
        // public ushort leaf;      // LF_METHODLIST [TYPTYPE]
        public byte[]   mList;      // really a mlMethod type
    };

    //  type record for LF_BITFIELD

    public struct LeafBitfield {
        // public ushort leaf;      // LF_BITFIELD [TYPTYPE]
        public uint     type;       // (type index) type of bitfield
        public byte     length;
        public byte     position;
    };

    //  type record for dimensioned array with constant bounds

    public struct LeafDimCon {
        // public ushort leaf;      // LF_DIMCONU or LF_DIMCONLU [TYPTYPE]
        public uint     typ;        // (type index) type of index
        public ushort   rank;       // number of dimensions
        public byte[]   dim;        // array of dimension information with
        // either upper bounds or lower/upper bound
    };

    //  type record for dimensioned array with variable bounds

    public struct LeafDimVar {
        // public ushort leaf;      // LF_DIMVARU or LF_DIMVARLU [TYPTYPE]
        public uint     rank;       // number of dimensions
        public uint     typ;        // (type index) type of index
        public uint[]   dim;        // (type index) array of type indices for either
        // variable upper bound or variable
        // lower/upper bound.  The count of type
        // indices is rank or rank*2 depending on
        // whether it is LFDIMVARU or LF_DIMVARLU.
        // The referenced types must be
        // LF_REFSYM or T_VOID
    };

    //  type record for referenced symbol

    public struct LeafRefSym {
        // public ushort leaf;      // LF_REFSYM [TYPTYPE]
        public byte[]   Sym;        // copy of referenced symbol record
        // (including length)
    };

    //  the following are numeric leaves.  They are used to indicate the
    //  size of the following variable length data.  When the numeric
    //  data is a single byte less than 0x8000, then the data is output
    //  directly.  If the data is more the 0x8000 or is a negative value,
    //  then the data is preceeded by the proper index.
    //

    //  signed character leaf

    public struct LeafChar {
        // public ushort leaf;      // LF_CHAR [TYPTYPE]
        public sbyte    val;        // signed 8-bit value
    };

    //  signed short leaf

    public struct LeafShort {
        // public ushort leaf;      // LF_SHORT [TYPTYPE]
        public short    val;        // signed 16-bit value
    };

    //  ushort leaf

    public struct LeafUShort {
        // public ushort leaf;      // LF_ushort [TYPTYPE]
        public ushort   val;        // unsigned 16-bit value
    };

    //  signed (32-bit) long leaf

    public struct LeafLong {
        // public ushort leaf;      // LF_LONG [TYPTYPE]
        public int      val;        // signed 32-bit value
    };

    //  uint    leaf

    public struct LeafULong {
        // public ushort leaf;      // LF_ULONG [TYPTYPE]
        public uint     val;        // unsigned 32-bit value
    };

    //  signed quad leaf

    public struct LeafQuad {
        // public ushort leaf;      // LF_QUAD [TYPTYPE]
        public long     val;        // signed 64-bit value
    };

    //  unsigned quad leaf

    public struct LeafUQuad {
        // public ushort leaf;      // LF_UQUAD [TYPTYPE]
        public ulong    val;        // unsigned 64-bit value
    };

    //  signed int128 leaf

    public struct LeafOct {
        // public ushort leaf;      // LF_OCT [TYPTYPE]
        public ulong    val0;
        public ulong    val1;       // signed 128-bit value
    };

    //  unsigned int128 leaf

    public struct LeafUOct {
        // public ushort leaf;      // LF_UOCT [TYPTYPE]
        public ulong    val0;
        public ulong    val1;       // unsigned 128-bit value
    };

    //  real 32-bit leaf

    public struct LeafReal32 {
        // public ushort leaf;      // LF_REAL32 [TYPTYPE]
        public float    val;        // 32-bit real value
    };

    //  real 64-bit leaf

    public struct LeafReal64 {
        // public ushort leaf;      // LF_REAL64 [TYPTYPE]
        public double   val;        // 64-bit real value
    };

    //  real 80-bit leaf

    public struct LeafReal80 {
        // public ushort leaf;      // LF_REAL80 [TYPTYPE]
        public FLOAT10  val;        // real 80-bit value
    };

    //  real 128-bit leaf

    public struct LeafReal128 {
        // public ushort leaf;      // LF_REAL128 [TYPTYPE]
        public ulong    val0;
        public ulong    val1;       // real 128-bit value
    };

    //  complex 32-bit leaf

    public struct LeafCmplx32 {
        // public ushort leaf;      // LF_COMPLEX32 [TYPTYPE]
        public float    val_real;   // real component
        public float    val_imag;   // imaginary component
    };

    //  complex 64-bit leaf

    public struct LeafCmplx64 {
        // public ushort leaf;      // LF_COMPLEX64 [TYPTYPE]
        public double   val_real;   // real component
        public double   val_imag;   // imaginary component
    };

    //  complex 80-bit leaf

    public struct LeafCmplx80 {
        // public ushort leaf;      // LF_COMPLEX80 [TYPTYPE]
        public FLOAT10  val_real;   // real component
        public FLOAT10  val_imag;   // imaginary component
    };

    //  complex 128-bit leaf

    public struct LeafCmplx128 {
        // public ushort leaf;      // LF_COMPLEX128 [TYPTYPE]
        public ulong    val0_real;
        public ulong    val1_real;  // real component
        public ulong    val0_imag;
        public ulong    val1_imag;  // imaginary component
    };

    //  variable length numeric field

    public struct LeafVarString {
        // public ushort leaf;      // LF_VARSTRING [TYPTYPE]
        public ushort   len;        // length of value in bytes
        public byte[]   value;      // value
    };

    //  index leaf - contains type index of another leaf
    //  a major use of this leaf is to allow the compilers to emit a
    //  long complex list (LF_FIELD) in smaller pieces.

    public struct LeafIndex {
        // public ushort leaf;      // LF_INDEX [TYPTYPE]
        public ushort   pad0;       // internal padding, must be 0
        public uint     index;      // (type index) type index of referenced leaf
    };

    //  subfield record for base class field

    public struct LeafBClass {
        // public ushort leaf;      // LF_BCLASS [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) attribute
        public uint     index;      // (type index) type index of base class
        public byte[]   offset;     // variable length offset of base within class
    };

    //  subfield record for direct and indirect virtual base class field

    public struct LeafVBClass {
        // public ushort leaf;      // LF_VBCLASS | LV_IVBCLASS [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) attribute
        public uint     index;      // (type index) type index of direct virtual base class
        public uint     vbptr;      // (type index) type index of virtual base pointer
        public byte[]   vbpoff;     // virtual base pointer offset from address point
        // followed by virtual base offset from vbtable
    };

    //  subfield record for friend class

    public struct LeafFriendCls {
        // public ushort leaf;      // LF_FRIENDCLS [TYPTYPE]
        public ushort   pad0;       // internal padding, must be 0
        public uint     index;      // (type index) index to type record of friend class
    };

    //  subfield record for friend function

    public struct LeafFriendFcn {
        // public ushort leaf;      // LF_FRIENDFCN [TYPTYPE]
        public ushort   pad0;       // internal padding, must be 0
        public uint     index;      // (type index) index to type record of friend function
        public string   name;       // name of friend function
    };

    //  subfield record for non-static data members

    public struct LeafMember {
        // public ushort leaf;      // LF_MEMBER [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t)attribute mask
        public uint     index;      // (type index) index of type record for field
        public byte[]   offset;     // variable length offset of field
        public string   name;       // length prefixed name of field
    };

    //  type record for static data members

    public struct LeafSTMember {
        // public ushort leaf;      // LF_STMEMBER [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) attribute mask
        public uint     index;      // (type index) index of type record for field
        public string   name;       // length prefixed name of field
    };

    //  subfield record for virtual function table pointer

    public struct LeafVFuncTab {
        // public ushort leaf;      // LF_VFUNCTAB [TYPTYPE]
        public ushort   pad0;       // internal padding, must be 0
        public uint     type;       // (type index) type index of pointer
    };

    //  subfield record for virtual function table pointer with offset

    public struct LeafVFuncOff {
        // public ushort leaf;      // LF_VFUNCOFF [TYPTYPE]
        public ushort   pad0;       // internal padding, must be 0.
        public uint     type;       // (type index) type index of pointer
        public int      offset;     // offset of virtual function table pointer
    };

    //  subfield record for overloaded method list

    public struct LeafMethod {
        // public ushort leaf;      // LF_METHOD [TYPTYPE]
        public ushort   count;      // number of occurrences of function
        public uint     mList;      // (type index) index to LF_METHODLIST record
        public string   name;       // length prefixed name of method
    };

    //  subfield record for nonoverloaded method

    public struct LeafOneMethod {
        // public ushort leaf;      // LF_ONEMETHOD [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) method attribute
        public uint     index;      // (type index) index to type record for procedure
        public uint[]   vbaseoff;   // offset in vfunctable if intro virtual
        public string   name;
    };

    //  subfield record for enumerate

    public struct LeafEnumerate {
        // public ushort leaf;      // LF_ENUMERATE [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) access
        public byte[]   value;      // variable length value field
        public string   name;
    };

    //  type record for nested (scoped) type definition

    public struct LeafNestType {
        // public ushort leaf;      // LF_NESTTYPE [TYPTYPE]
        public ushort   pad0;       // internal padding, must be 0
        public uint     index;      // (type index) index of nested type definition
        public string   name;       // length prefixed type name
    };

    //  type record for nested (scoped) type definition, with attributes
    //  new records for vC v5.0, no need to have 16-bit ti versions.

    public struct LeafNestTypeEx {
        // public ushort leaf;      // LF_NESTTYPEEX [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) member access
        public uint     index;      // (type index) index of nested type definition
        public string   name;       // length prefixed type name
    };

    //  type record for modifications to members

    public struct LeafMemberModify {
        // public ushort leaf;      // LF_MEMBERMODIFY [TYPTYPE]
        public ushort   attr;       // (CV_fldattr_t) the new attributes
        public uint     index;      // (type index) index of base class type definition
        public string   name;       // length prefixed member name
    };

    //  type record for pad leaf

    public struct LeafPad {
        public byte     leaf;
    };

    //  Symbol definitions

    public enum SYM {
        S_END           =  0x0006,  // Block, procedure, "with" or thunk end
        S_OEM           =  0x0404,  // OEM defined symbol

        S_REGISTER_ST   =  0x1001,  // Register variable
        S_CONSTANT_ST   =  0x1002,  // constant symbol
        S_UDT_ST        =  0x1003,  // User defined type
        S_COBOLUDT_ST   =  0x1004,  // special UDT for cobol that does not symbol pack
        S_MANYREG_ST    =  0x1005,  // multiple register variable
        S_BPREL32_ST    =  0x1006,  // BP-relative
        S_LDATA32_ST    =  0x1007,  // Module-local symbol
        S_GDATA32_ST    =  0x1008,  // Global data symbol
        S_PUB32_ST      =  0x1009,  // a public symbol (CV internal reserved)
        S_LPROC32_ST    =  0x100a,  // Local procedure start
        S_GPROC32_ST    =  0x100b,  // Global procedure start
        S_VFTABLE32     =  0x100c,  // address of virtual function table
        S_REGREL32_ST   =  0x100d,  // register relative address
        S_LTHREAD32_ST  =  0x100e,  // local thread storage
        S_GTHREAD32_ST  =  0x100f,  // global thread storage

        S_LPROCMIPS_ST  =  0x1010,  // Local procedure start
        S_GPROCMIPS_ST  =  0x1011,  // Global procedure start

        // new symbol records for edit and continue information

        S_FRAMEPROC     =  0x1012,  // extra frame and proc information
        S_COMPILE2_ST   =  0x1013,  // extended compile flags and info

        // new symbols necessary for 16-bit enumerates of IA64 registers
        // and IA64 specific symbols

        S_MANYREG2_ST   =  0x1014,  // multiple register variable
        S_LPROCIA64_ST  =  0x1015,  // Local procedure start (IA64)
        S_GPROCIA64_ST  =  0x1016,  // Global procedure start (IA64)

        // Local symbols for IL
        S_LOCALSLOT_ST  =  0x1017,  // local IL sym with field for local slot index
        S_PARAMSLOT_ST  =  0x1018,  // local IL sym with field for parameter slot index

        S_ANNOTATION    =  0x1019,  // Annotation string literals

        // symbols to support managed code debugging
        S_GMANPROC_ST   =  0x101a,  // Global proc
        S_LMANPROC_ST   =  0x101b,  // Local proc
        S_RESERVED1     =  0x101c,  // reserved
        S_RESERVED2     =  0x101d,  // reserved
        S_RESERVED3     =  0x101e,  // reserved
        S_RESERVED4     =  0x101f,  // reserved
        S_LMANDATA_ST   =  0x1020,
        S_GMANDATA_ST   =  0x1021,
        S_MANFRAMEREL_ST=  0x1022,
        S_MANREGISTER_ST=  0x1023,
        S_MANSLOT_ST    =  0x1024,
        S_MANMANYREG_ST =  0x1025,
        S_MANREGREL_ST  =  0x1026,
        S_MANMANYREG2_ST=  0x1027,
        S_MANTYPREF     =  0x1028,  // Index for type referenced by name from metadata
        S_UNAMESPACE_ST =  0x1029,  // Using namespace

        // Symbols w/ SZ name fields. All name fields contain utf8 encoded strings.
        S_ST_MAX        =  0x1100,  // starting point for SZ name symbols

        S_OBJNAME       =  0x1101,  // path to object file name
        S_THUNK32       =  0x1102,  // Thunk Start
        S_BLOCK32       =  0x1103,  // block start
        S_WITH32        =  0x1104,  // with start
        S_LABEL32       =  0x1105,  // code label
        S_REGISTER      =  0x1106,  // Register variable
        S_CONSTANT      =  0x1107,  // constant symbol
        S_UDT           =  0x1108,  // User defined type
        S_COBOLUDT      =  0x1109,  // special UDT for cobol that does not symbol pack
        S_MANYREG       =  0x110a,  // multiple register variable
        S_BPREL32       =  0x110b,  // BP-relative
        S_LDATA32       =  0x110c,  // Module-local symbol
        S_GDATA32       =  0x110d,  // Global data symbol
        S_PUB32         =  0x110e,  // a public symbol (CV internal reserved)
        S_LPROC32       =  0x110f,  // Local procedure start
        S_GPROC32       =  0x1110,  // Global procedure start
        S_REGREL32      =  0x1111,  // register relative address
        S_LTHREAD32     =  0x1112,  // local thread storage
        S_GTHREAD32     =  0x1113,  // global thread storage

        S_LPROCMIPS     =  0x1114,  // Local procedure start
        S_GPROCMIPS     =  0x1115,  // Global procedure start
        S_COMPILE2      =  0x1116,  // extended compile flags and info
        S_MANYREG2      =  0x1117,  // multiple register variable
        S_LPROCIA64     =  0x1118,  // Local procedure start (IA64)
        S_GPROCIA64     =  0x1119,  // Global procedure start (IA64)
        S_LOCALSLOT     =  0x111a,  // local IL sym with field for local slot index
        S_SLOT          = S_LOCALSLOT,  // alias for LOCALSLOT
        S_PARAMSLOT     =  0x111b,  // local IL sym with field for parameter slot index

        // symbols to support managed code debugging
        S_LMANDATA      =  0x111c,
        S_GMANDATA      =  0x111d,
        S_MANFRAMEREL   =  0x111e,
        S_MANREGISTER   =  0x111f,
        S_MANSLOT       =  0x1120,
        S_MANMANYREG    =  0x1121,
        S_MANREGREL     =  0x1122,
        S_MANMANYREG2   =  0x1123,
        S_UNAMESPACE    =  0x1124,  // Using namespace

        // ref symbols with name fields
        S_PROCREF       =  0x1125,  // Reference to a procedure
        S_DATAREF       =  0x1126,  // Reference to data
        S_LPROCREF      =  0x1127,  // Local Reference to a procedure
        S_ANNOTATIONREF =  0x1128,  // Reference to an S_ANNOTATION symbol
        S_TOKENREF      =  0x1129,  // Reference to one of the many MANPROCSYM's

        // continuation of managed symbols
        S_GMANPROC      =  0x112a,  // Global proc
        S_LMANPROC      =  0x112b,  // Local proc

        // short, light-weight thunks
        S_TRAMPOLINE    =  0x112c,  // trampoline thunks
        S_MANCONSTANT   =  0x112d,  // constants with metadata type info

        // native attributed local/parms
        S_ATTR_FRAMEREL =  0x112e,  // relative to virtual frame ptr
        S_ATTR_REGISTER =  0x112f,  // stored in a register
        S_ATTR_REGREL   =  0x1130,  // relative to register (alternate frame ptr)
        S_ATTR_MANYREG  =  0x1131,  // stored in >1 register

        // Separated code (from the compiler) support
        S_SEPCODE       =  0x1132,

        S_LOCAL         =  0x1133,  // defines a local symbol in optimized code
        S_DEFRANGE      =  0x1134,  // defines a single range of addresses in which symbol can be evaluated
        S_DEFRANGE2     =  0x1135,  // defines ranges of addresses in which symbol can be evaluated

        S_SECTION       =  0x1136,  // A COFF section in a PE executable
        S_COFFGROUP     =  0x1137,  // A COFF group
        S_EXPORT        =  0x1138,  // A export

        S_CALLSITEINFO  =  0x1139,  // Indirect call site information
        S_FRAMECOOKIE   =  0x113a,  // Security cookie information

        S_DISCARDED     =  0x113b,  // Discarded by LINK /OPT:REF (experimental, see richards)

        S_RECTYPE_MAX,              // one greater than last
        S_RECTYPE_LAST  = S_RECTYPE_MAX - 1,

    };

    //  enum describing compile flag ambient data model

    public enum CV_CFL_DATA {
        CV_CFL_DNEAR    = 0x00,
        CV_CFL_DFAR     = 0x01,
        CV_CFL_DHUGE    = 0x02
    };

    //  enum describing compile flag ambiant code model

    public enum CV_CFL_CODE {
        CV_CFL_CNEAR    = 0x00,
        CV_CFL_CFAR     = 0x01,
        CV_CFL_CHUGE    = 0x02
    };

    //  enum describing compile flag target floating point package

    public enum CV_CFL_FPKG {
        CV_CFL_NDP      = 0x00,
        CV_CFL_EMU      = 0x01,
        CV_CFL_ALT      = 0x02
    };

    // enum describing function return method

    [Flags]
    public enum CV_PROCFLAGS : byte {
        CV_PFLAG_NOFPO      = 0x01, // frame pointer present
        CV_PFLAG_INT        = 0x02, // interrupt return
        CV_PFLAG_FAR        = 0x04, // far return
        CV_PFLAG_NEVER      = 0x08, // function does not return
        CV_PFLAG_NOTREACHED = 0x10, // label isn't fallen into
        CV_PFLAG_CUST_CALL  = 0x20, // custom calling convention
        CV_PFLAG_NOINLINE   = 0x40, // function marked as noinline
        CV_PFLAG_OPTDBGINFO = 0x80, // function has debug information for optimized code
    };

    // Extended proc flags
    //
    public struct CV_EXPROCFLAGS {
        public byte     flags;      // (CV_PROCFLAGS)
        public byte     reserved;   // must be zero
    };

    // local variable flags
    [Flags]
    public enum CV_LVARFLAGS : ushort {
        fIsParam        = 0x0001,   // variable is a parameter
        fAddrTaken      = 0x0002,   // address is taken
        fCompGenx       = 0x0004,   // variable is compiler generated
        fIsAggregate    = 0x0008,   // the symbol is splitted in temporaries,
        // which are treated by compiler as
        // independent entities
        fIsAggregated   = 0x0010,   // Counterpart of fIsAggregate - tells
        // that it is a part of a fIsAggregate symbol
        fIsAliased      = 0x0020,   // variable has multiple simultaneous lifetimes
        fIsAlias        = 0x0040,   // represents one of the multiple simultaneous lifetimes
    };

    // represents an address range, used for optimized code debug info
    public struct CV_lvar_addr_range {       // defines a range of addresses
        public uint     offStart;
        public ushort   isectStart;
        public uint     cbRange;
    };

    // enum describing function data return method

    public enum CV_GENERIC_STYLE {
        CV_GENERIC_VOID   = 0x00,   // void return type
        CV_GENERIC_REG    = 0x01,   // return data is in registers
        CV_GENERIC_ICAN   = 0x02,   // indirect caller allocated near
        CV_GENERIC_ICAF   = 0x03,   // indirect caller allocated far
        CV_GENERIC_IRAN   = 0x04,   // indirect returnee allocated near
        CV_GENERIC_IRAF   = 0x05,   // indirect returnee allocated far
        CV_GENERIC_UNUSED = 0x06    // first unused
    };

    [Flags]
    public enum CV_GENERIC_FLAG : ushort {
        cstyle      = 0x0001,       // true push varargs right to left
        rsclean     = 0x0002,       // true if returnee stack cleanup
    };

    // flag bitfields for separated code attributes

    [Flags]
    public enum CV_SEPCODEFLAGS : uint {
        fIsLexicalScope     = 0x00000001,   // S_SEPCODE doubles as lexical scope
        fReturnsToParent    = 0x00000002,   // code frag returns to parent
    };

    // Generic layout for symbol records

    public struct SYMTYPE {
        public ushort   reclen;     // Record length
        public ushort   rectyp;     // Record type
        // byte        data[CV_ZEROLEN];
        //  SYMTYPE *NextSym (SYMTYPE * pSym) {
        //  return (SYMTYPE *) ((char *)pSym + pSym->reclen + sizeof(ushort));
        //  }
    };

    //  non-model specific symbol types

    public struct RegSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_REGISTER
        public uint     typind;     // (type index) Type index or Metadata token
        public ushort   reg;        // register enumerate
        public string   name;       // Length-prefixed name
    };

    public struct AttrRegSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANREGISTER | S_ATTR_REGISTER
        public uint     typind;     // (type index) Type index or Metadata token
        public uint     offCod;     // first code address where var is live
        public ushort   segCod;
        public ushort   flags;      // (CV_LVARFLAGS)local var flags
        public ushort   reg;        // register enumerate
        public string   name;       // Length-prefixed name
    };

    public struct ManyRegSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANYREG
        public uint     typind;     // (type index) Type index or metadata token
        public byte     count;      // count of number of registers
        public byte[]   reg;        // count register enumerates, most-sig first
        public string   name;       // length-prefixed name.
    };

    public struct ManyRegSym2 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANYREG2
        public uint     typind;     // (type index) Type index or metadata token
        public ushort   count;      // count of number of registers,
        public ushort[] reg;        // count register enumerates, most-sig first
        public string   name;       // length-prefixed name.
    };

    public struct AttrManyRegSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANMANYREG
        public uint     typind;     // (type index) Type index or metadata token
        public uint     offCod;     // first code address where var is live
        public ushort   segCod;
        public ushort   flags;      // (CV_LVARFLAGS)local var flags
        public byte     count;      // count of number of registers
        public byte[]   reg;        // count register enumerates, most-sig first
        public string   name;       // utf-8 encoded zero terminate name
    };

    public struct AttrManyRegSym2 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANMANYREG2 | S_ATTR_MANYREG
        public uint     typind;     // (type index) Type index or metadata token
        public uint     offCod;     // first code address where var is live
        public ushort   segCod;
        public ushort   flags;      // (CV_LVARFLAGS)local var flags
        public ushort   count;      // count of number of registers
        public ushort[] reg;        // count register enumerates, most-sig first
        public string   name;       // utf-8 encoded zero terminate name
    };

    public struct ConstSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_CONSTANT or S_MANCONSTANT
        public uint     typind;     // (type index) Type index (containing enum if enumerate) or metadata token
        public ushort   value;      // numeric leaf containing value
        public string   name;       // Length-prefixed name
    };

    public struct UdtSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_UDT | S_COBOLUDT
        public uint     typind;     // (type index) Type index
        public string   name;       // Length-prefixed name
    };

    public struct ManyTypRef {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANTYPREF
        public uint     typind;     // (type index) Type index
    };

    public struct SearchSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_SSEARCH
        public uint     startsym;   // offset of the procedure
        public ushort   seg;        // segment of symbol
    };

    [Flags]
    public enum CFLAGSYM_FLAGS : ushort {
        pcode           = 0x0001,   // true if pcode present
        floatprec       = 0x0006,   // floating precision
        floatpkg        = 0x0018,   // float package
        ambdata         = 0x00e0,   // ambient data model
        ambcode         = 0x0700,   // ambient code model
        mode32          = 0x0800,   // true if compiled 32 bit mode
    };

    public struct CFlagSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_COMPILE
        public byte     machine;    // target processor
        public byte     language;   // language index
        public ushort   flags;      // (CFLAGSYM_FLAGS)
        public string   ver;        // Length-prefixed compiler version string
    };

    [Flags]
    public enum COMPILESYM_FLAGS : uint {
        iLanguage       = 0x000000ff,   // language index
        fEC             = 0x00000100,   // compiled for E/C
        fNoDbgInfo      = 0x00000200,   // not compiled with debug info
        fLTCG           = 0x00000400,   // compiled with LTCG
        fNoDataAlign    = 0x00000800,   // compiled with -Bzalign
        fManagedPresent = 0x00001000,   // managed code/data present
        fSecurityChecks = 0x00002000,   // compiled with /GS
        fHotPatch       = 0x00004000,   // compiled with /hotpatch
        fCVTCIL         = 0x00008000,   // converted with CVTCIL
        fMSILModule     = 0x00010000,   // MSIL netmodule
    };

    public struct CompileSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_COMPILE2
        public uint     flags;      // (COMPILESYM_FLAGS)
        public ushort   machine;    // target processor
        public ushort   verFEMajor; // front end major version #
        public ushort   verFEMinor; // front end minor version #
        public ushort   verFEBuild; // front end build version #
        public ushort   verMajor;   // back end major version #
        public ushort   verMinor;   // back end minor version #
        public ushort   verBuild;   // back end build version #
        public string   verSt;      // Length-prefixed compiler version string, followed
        public string[] verArgs;    // block of zero terminated strings, ended by double-zero.
    };

    public struct ObjNameSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_OBJNAME
        public uint     signature;  // signature
        public string   name;       // Length-prefixed name
    };

    public struct EndArgSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_ENDARG
    };

    public struct ReturnSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_RETURN
        public CV_GENERIC_FLAG flags; // flags
        public byte     style;      // CV_GENERIC_STYLE return style
        // followed by return method data
    };

    public struct EntryThisSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_ENTRYTHIS
        public byte     thissym;    // symbol describing this pointer on entry
    };

    public struct BpRelSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_BPREL32
        public int      off;        // BP-relative offset
        public uint     typind;     // (type index) Type index or Metadata token
        public string   name;       // Length-prefixed name
    };

    public struct FrameRelSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANFRAMEREL | S_ATTR_FRAMEREL
        public int      off;        // Frame relative offset
        public uint     typind;     // (type index) Type index or Metadata token
        public uint     offCod;     // first code address where var is live
        public ushort   segCod;
        public ushort   flags;      // (CV_LVARFLAGS)local var flags
        public string   name;       // Length-prefixed name
    };

    public struct SlotSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_LOCALSLOT or S_PARAMSLOT
        public uint     index;      // slot index
        public uint     typind;     // (type index) Type index or Metadata token
        public string   name;       // Length-prefixed name
    };

    public struct AttrSlotSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANSLOT
        public uint     index;      // slot index
        public uint     typind;     // (type index) Type index or Metadata token
        public uint     offCod;     // first code address where var is live
        public ushort   segCod;
        public ushort   flags;      // (CV_LVARFLAGS)local var flags
        public string   name;       // Length-prefixed name

    };

    public struct AnnotationSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_ANNOTATION
        public uint     off;
        public ushort   seg;
        public ushort   csz;        // Count of zero terminated annotation strings
        public string[] rgsz;       // Sequence of zero terminated annotation strings
    };

    public struct DatasSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_LDATA32, S_GDATA32 or S_PUB32, S_LMANDATA, S_GMANDATA
        public uint     typind;     // (type index) Type index, or Metadata token if a managed symbol
        public uint     off;
        public ushort   seg;
        public string   name;       // Length-prefixed name
    };

    [Flags]
    public enum CV_PUBSYMFLAGS : uint
    {
        fNone     = 0,
        fCode     = 0x00000001,     // set if public symbol refers to a code address
        fFunction = 0x00000002,     // set if public symbol is a function
        fManaged  = 0x00000004,     // set if managed code (native or IL)
        fMSIL     = 0x00000008,     // set if managed IL code
    };

    public struct PubSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_PUB32
        public uint     flags;      // (CV_PUBSYMFLAGS)
        public uint     off;
        public ushort   seg;
        public string   name;       // Length-prefixed name
    };

    public struct ProcSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_GPROC32 or S_LPROC32
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     next;       // pointer to next symbol
        public uint     len;        // Proc length
        public uint     dbgStart;   // Debug start offset
        public uint     dbgEnd;     // Debug end offset
        public uint     typind;     // (type index) Type index
        public uint     off;
        public ushort   seg;
        public byte     flags;      // (CV_PROCFLAGS) Proc flags
        public string   name;       // Length-prefixed name
    };

    public struct ManProcSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_GMANPROC, S_LMANPROC, S_GMANPROCIA64 or S_LMANPROCIA64
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     next;       // pointer to next symbol
        public uint     len;        // Proc length
        public uint     dbgStart;   // Debug start offset
        public uint     dbgEnd;     // Debug end offset
        public uint     token;      // COM+ metadata token for method
        public uint     off;
        public ushort   seg;
        public byte     flags;      // (CV_PROCFLAGS) Proc flags
        public ushort   retReg;     // Register return value is in (may not be used for all archs)
        public string   name;       // optional name field
    };

    public struct ManProcSymMips {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_GMANPROCMIPS or S_LMANPROCMIPS
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     next;       // pointer to next symbol
        public uint     len;        // Proc length
        public uint     dbgStart;   // Debug start offset
        public uint     dbgEnd;     // Debug end offset
        public uint     regSave;    // int register save mask
        public uint     fpSave;     // fp register save mask
        public uint     intOff;     // int register save offset
        public uint     fpOff;      // fp register save offset
        public uint     token;      // COM+ token type
        public uint     off;
        public ushort   seg;
        public byte     retReg;     // Register return value is in
        public byte     frameReg;   // Frame pointer register
        public string   name;       // optional name field
    };

    public struct ThunkSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_THUNK32
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     next;       // pointer to next symbol
        public uint     off;
        public ushort   seg;
        public ushort   len;        // length of thunk
        public byte     ord;        // THUNK_ORDINAL specifying type of thunk
        public string   name;       // Length-prefixed name
        public byte[]   variant;    // variant portion of thunk
    };

    public enum TRAMP {             // Trampoline subtype
        trampIncremental,           // incremental thunks
        trampBranchIsland,          // Branch island thunks
    };

    public struct TrampolineSym {   // Trampoline thunk symbol
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_TRAMPOLINE
        public ushort   trampType;  // trampoline sym subtype
        public ushort   cbThunk;    // size of the thunk
        public uint     offThunk;   // offset of the thunk
        public uint     offTarget;  // offset of the target of the thunk
        public ushort   sectThunk;  // section index of the thunk
        public ushort   sectTarget; // section index of the target of the thunk
    };

    public struct LabelSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_LABEL32
        public uint     off;
        public ushort   seg;
        public byte     flags;      // (CV_PROCFLAGS) flags
        public string   name;       // Length-prefixed name
    };

    public struct BlockSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_BLOCK32
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     len;        // Block length
        public uint     off;        // Offset in code segment
        public ushort   seg;        // segment of label
        public string   name;       // Length-prefixed name
    };

    public struct WithSym32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_WITH32
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     len;        // Block length
        public uint     off;        // Offset in code segment
        public ushort   seg;        // segment of label
        public string   expr;       // Length-prefixed expression string
    };

    public struct VpathSym32 {
        // public ushort reclen;    // record length
        // public ushort rectyp;    // S_VFTABLE32
        public uint     root;       // (type index) type index of the root of path
        public uint     path;       // (type index) type index of the path record
        public uint     off;        // offset of virtual function table
        public ushort   seg;        // segment of virtual function table
    };

    public struct RegRel32 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_REGREL32
        public uint     off;        // offset of symbol
        public uint     typind;     // (type index) Type index or metadata token
        public ushort   reg;        // register index for symbol
        public string   name;       // Length-prefixed name
    };

    public struct AttrRegRel {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_MANREGREL | S_ATTR_REGREL
        public uint     off;        // offset of symbol
        public uint     typind;     // (type index) Type index or metadata token
        public ushort   reg;        // register index for symbol
        public uint     offCod;     // first code address where var is live
        public ushort   segCod;
        public ushort   flags;      // (CV_LVARFLAGS)local var flags
        public string   name;       // Length-prefixed name
    };

    public struct ThreadSym32 {
        // public ushort reclen;    // record length
        // public ushort rectyp;    // S_LTHREAD32 | S_GTHREAD32
        public uint     typind;     // (type index) type index
        public uint     off;        // offset into thread storage
        public ushort   seg;        // segment of thread storage
        public string   name;       // length prefixed name
    };

    public struct Slink32 {
        // public ushort reclen;    // record length
        // public ushort rectyp;    // S_SLINK32
        public uint     framesize;  // frame size of parent procedure
        public int      off;        // signed offset where the static link was saved relative to the value of reg
        public ushort   reg;
    };

    public struct ProcSymMips {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_GPROCMIPS or S_LPROCMIPS
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     next;       // pointer to next symbol
        public uint     len;        // Proc length
        public uint     dbgStart;   // Debug start offset
        public uint     dbgEnd;     // Debug end offset
        public uint     regSave;    // int register save mask
        public uint     fpSave;     // fp register save mask
        public uint     intOff;     // int register save offset
        public uint     fpOff;      // fp register save offset
        public uint     typind;     // (type index) Type index
        public uint     off;        // Symbol offset
        public ushort   seg;        // Symbol segment
        public byte     retReg;     // Register return value is in
        public byte     frameReg;   // Frame pointer register
        public string   name;       // Length-prefixed name
    };

    public struct ProcSymIa64 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_GPROCIA64 or S_LPROCIA64
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this blocks end
        public uint     next;       // pointer to next symbol
        public uint     len;        // Proc length
        public uint     dbgStart;   // Debug start offset
        public uint     dbgEnd;     // Debug end offset
        public uint     typind;     // (type index) Type index
        public uint     off;        // Symbol offset
        public ushort   seg;        // Symbol segment
        public ushort   retReg;     // Register return value is in
        public byte     flags;      // (CV_PROCFLAGS) Proc flags
        public string   name;       // Length-prefixed name
    };

    public struct RefSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_PROCREF_ST, S_DATAREF_ST, or S_LPROCREF_ST
        public uint     sumName;    // SUC of the name
        public uint     ibSym;      // Offset of actual symbol in $$Symbols
        public ushort   imod;       // Module containing the actual symbol
        public ushort   usFill;     // align this record
    };

    public struct RefSym2 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_PROCREF, S_DATAREF, or S_LPROCREF
        public uint     sumName;    // SUC of the name
        public uint     ibSym;      // Offset of actual symbol in $$Symbols
        public ushort   imod;       // Module containing the actual symbol
        public string   name;       // hidden name made a first class member
    };

    public struct AlignSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_ALIGN
    };

    public struct OemSymbol {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_OEM
        public Guid     idOem;      // an oem ID (GUID)
        public uint     typind;     // (type index) Type index
        public byte[]   rgl;        // user data, force 4-byte alignment
    };

    [Flags]
    public enum FRAMEPROCSYM_FLAGS : uint {
        fHasAlloca          = 0x00000001,   // function uses _alloca()
        fHasSetJmp          = 0x00000002,   // function uses setjmp()
        fHasLongJmp         = 0x00000004,   // function uses longjmp()
        fHasInlAsm          = 0x00000008,   // function uses inline asm
        fHasEH              = 0x00000010,   // function has EH states
        fInlSpec            = 0x00000020,   // function was speced as inline
        fHasSEH             = 0x00000040,   // function has SEH
        fNaked              = 0x00000080,   // function is __declspec(naked)
        fSecurityChecks     = 0x00000100,   // function has buffer security check introduced by /GS.
        fAsyncEH            = 0x00000200,   // function compiled with /EHa
        fGSNoStackOrdering  = 0x00000400,   // function has /GS buffer checks, but stack ordering couldn't be done
        fWasInlined         = 0x00000800,   // function was inlined within another function
    };

    public struct FrameProcSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_FRAMEPROC
        public uint     cbFrame;    // count of bytes of total frame of procedure
        public uint     cbPad;      // count of bytes of padding in the frame
        public uint     offPad;     // offset (rel to frame) to where padding starts
        public uint     cbSaveRegs; // count of bytes of callee save registers
        public uint     offExHdlr;  // offset of exception handler
        public ushort   secExHdlr;  // section id of exception handler
        public uint     flags;      // (FRAMEPROCSYM_FLAGS)
    }

    public struct UnamespaceSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_UNAMESPACE
        public string   name;       // name
    };

    public struct SepCodSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_SEPCODE
        public uint     parent;     // pointer to the parent
        public uint     end;        // pointer to this block's end
        public uint     length;     // count of bytes of this block
        public uint     scf;        // (CV_SEPCODEFLAGS) flags
        public uint     off;        // sec:off of the separated code
        public uint     offParent;  // secParent:offParent of the enclosing scope
        public ushort   sec;        //  (proc, block, or sepcode)
        public ushort   secParent;
    };

    public struct LocalSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_LOCAL
        public uint     id;         // id of the local
        public uint     typind;     // (type index) type index
        public ushort   flags;      // (CV_LVARFLAGS) local var flags
        public uint     idParent;   // This is is parent variable - fIsAggregated or fIsAlias
        public uint     offParent;  // Offset in parent variable - fIsAggregated

        public uint     expr;       // NI of expression that this temp holds
        public uint     pad0;       // pad, must be zero
        public uint     pad1;       // pad, must be zero

        public string   name;       // Name of this symbol.
    }

    public struct DefRangeSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_DEFRANGE

        public uint     id;         // ID of the local symbol for which this formula holds
        public uint     program;    // program to evaluate the value of the symbol

        public CV_lvar_addr_range range;   // Range of addresses where this program is valid
    };

    public struct DefRangeSym2 {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_DEFRANGE2

        public uint     id;         // ID of the local symbol for which this formula holds
        public uint     program;    // program to evaluate the value of the symbol

        public ushort   count;      // count of CV_lvar_addr_range records following
        public CV_lvar_addr_range[] range;// Range of addresses where this program is valid
    };

    public struct SectionSym {
        // public ushort reclen     // Record length
        // public ushort rectyp;    // S_SECTION

        public ushort   isec;       // Section number
        public byte     align;      // Alignment of this section (power of 2)
        public byte     bReserved;  // Reserved.  Must be zero.
        public uint     rva;
        public uint     cb;
        public uint     characteristics;
        public string   name;       // name
    };

    public struct CoffGroupSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_COFFGROUP

        public uint     cb;
        public uint     characteristics;
        public uint     off;        // Symbol offset
        public ushort   seg;        // Symbol segment
        public string   name;       // name
    };

    [Flags]
    public enum EXPORTSYM_FLAGS : ushort {
        fConstant       = 0x0001,   // CONSTANT
        fData           = 0x0002,   // DATA
        fPrivate        = 0x0004,   // PRIVATE
        fNoName         = 0x0008,   // NONAME
        fOrdinal        = 0x0010,   // Ordinal was explicitly assigned
        fForwarder      = 0x0020,   // This is a forwarder
    }

    public struct ExportSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_EXPORT

        public ushort   ordinal;
        public ushort   flags;      // (EXPORTSYM_FLAGS)
        public string   name;       // name of
    };

    //
    // Symbol for describing indirect calls when they are using
    // a function pointer cast on some other type or temporary.
    // Typical content will be an LF_POINTER to an LF_PROCEDURE
    // type record that should mimic an actual variable with the
    // function pointer type in question.
    //
    // Since the compiler can sometimes tail-merge a function call
    // through a function pointer, there may be more than one
    // S_CALLSITEINFO record at an address.  This is similar to what
    // you could do in your own code by:
    //
    //  if (expr)
    //  pfn = &function1;
    //  else
    //  pfn = &function2;
    //
    //  (*pfn)(arg list);
    //

    public struct CallsiteInfo {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_CALLSITEINFO
        public int      off;        // offset of call site
        public ushort   ect;        // section index of call site
        public ushort   pad0;       // alignment padding field, must be zero
        public uint     typind;     // (type index) type index describing function signature
    };

    // Frame cookie information

    public enum CV_cookietype
    {
        CV_COOKIETYPE_COPY = 0,
        CV_COOKIETYPE_XOR_SP,
        CV_COOKIETYPE_XOR_BP,
        CV_COOKIETYPE_XOR_R13,
    };

    // Symbol for describing security cookie's position and type
    // (raw, xor'd with esp, xor'd with ebp).

    public struct FrameCookie {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_FRAMECOOKIE
        public int      off;        // Frame relative offset
        public ushort   reg;        // Register index
        public int      cookietype; // (CV_cookietype) Type of the cookie
        public byte     flags;      // Flags describing this cookie
    };

    public enum CV_DISCARDED : uint
    {
        CV_DISCARDED_UNKNOWN = 0,
        CV_DISCARDED_NOT_SELECTED = 1,
        CV_DISCARDED_NOT_REFERENCED = 2,
    };

    public struct DiscardedSym {
        // public ushort reclen;    // Record length [SYMTYPE]
        // public ushort rectyp;    // S_DISCARDED
        public CV_DISCARDED iscarded;
        public uint     fileid;     // First FILEID if line number info present
        public uint     linenum;    // First line number
        public byte[]   data;       // Original record(s) with invalid indices
    };

    //
    // V7 line number data types
    //

    public enum DEBUG_S_SUBSECTION_TYPE : uint {
        DEBUG_S_IGNORE      = 0x80000000,   // if this bit is set in a subsection type then ignore the subsection contents

        DEBUG_S_SYMBOLS     = 0xf1,
        DEBUG_S_LINES       = 0xf2,
        DEBUG_S_STRINGTABLE = 0xf3,
        DEBUG_S_FILECHKSMS  = 0xf4,
        DEBUG_S_FRAMEDATA   = 0xf5,
    };

    //
    // Line flags (data present)
    //
    public enum CV_LINE_SUBSECTION_FLAGS : ushort {
        CV_LINES_HAVE_COLUMNS = 0x0001,
    }

    public struct CV_LineSection {
        public uint     off;
        public ushort   sec;
        public ushort   flags;
        public uint     cod;
    }

    public struct CV_SourceFile {
        public uint     index;          // Index to file in checksum section.
        public uint     count;          // Number of CV_Line records.
        public uint     linsiz;         // Size of CV_Line recods.
    }

    [Flags]
    public enum CV_Line_Flags : uint {
        linenumStart    = 0x00ffffff,   // line where statement/expression starts
        deltaLineEnd    = 0x7f000000,   // delta to line where statement ends (optional)
        fStatement      = 0x80000000,   // true if a statement linenumber, else an expression line num
    };

    public struct CV_Line {
        public uint     offset;         // Offset to start of code bytes for line number
        public uint     flags;          // (CV_Line_Flags)
    };

    public struct CV_Column {
        public ushort   offColumnStart;
        public ushort   offColumnEnd;
    };

    //  File information

    public enum CV_FILE_CHECKSUM_TYPE : byte {
        None    = 0,
        MD5     = 1,
    };

    public struct CV_FileCheckSum {
        public uint     name;           // Index of name in name table.
        public byte     len;            // Hash length
        public byte     type;           // Hash type
    }

    [Flags]
    public enum FRAMEDATA_FLAGS : uint
    {
        fHasSEH             = 0x00000001,
        fHasEH              = 0x00000002,
        fIsFunctionStart    = 0x00000004,
    };

    public struct FrameData {
        public uint     ulRvaStart;
        public uint     cbBlock;
        public uint     cbLocals;
        public uint     cbParams;
        public uint     cbStkMax;
        public uint     frameFunc;
        public ushort   cbProlog;
        public ushort   cbSavedRegs;
        public uint     flags;          // (FRAMEDATA_FLAGS)
    };

    public struct XFixupData {
        public ushort   wType;
        public ushort   wExtra;
        public uint     rva;
        public uint     rvaTarget;
    };

    public enum DEBUG_S_SUBSECTION {
        SYMBOLS         = 0xF1,
        LINES           = 0xF2,
        STRINGTABLE     = 0xF3,
        FILECHKSMS      = 0xF4,
        FRAMEDATA       = 0xF5,
    }
}
#endif