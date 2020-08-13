using System;

namespace FixPoint
{
    public struct VMatrix3x3
    {
        public static readonly VMatrix3x3
            zero = new VMatrix3x3(VInt3.zero, VInt3.zero, VInt3.zero);

        public static readonly VMatrix3x3 identity = new VMatrix3x3(new VInt3(1000, 0, 0),
            new VInt3(0, 1000, 0), new VInt3( 0, 0, 1000));

        // mRowCol  列优先存储
        public int m00;
        public int m10;
        public int m20;
        public int m01;
        public int m11;
        public int m21;
        public int m02;
        public int m12;
        public int m22;

        public VMatrix3x3(VInt3 column0, VInt3 column1, VInt3 column2)
        {
            this.m00 = column0.x;
            this.m01 = column1.x;
            this.m02 = column2.x;
            this.m10 = column0.y;
            this.m11 = column1.y;
            this.m12 = column2.y;
            this.m20 = column0.z;
            this.m21 = column1.z;
            this.m22 = column2.z;
        }


        public VInt this[int row, int column]
        {
            get { return this[row + column * 3]; }
            set { this[row + column * 3] = value; }
        }

        public VInt this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return new VInt(this.m00);
                    case 1:
                        return new VInt(this.m10);
                    case 2:
                        return new VInt(this.m20);
                    case 3:
                        return new VInt(this.m01);
                    case 4:
                        return new VInt(this.m11);
                    case 5:
                        return new VInt(this.m21);
                    case 6:
                        return new VInt(this.m02);
                    case 7:
                        return new VInt(this.m12);
                    case 8:
                        return new VInt(this.m22);
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.m00 = value.i;
                        break;
                    case 1:
                        this.m10 = value.i;
                        break;
                    case 2:
                        this.m20 = value.i;
                        break;
                    case 3:
                        this.m01 = value.i;
                        break;
                    case 4:
                        this.m11 = value.i;
                        break;
                    case 5:
                        this.m21 = value.i;
                        break;
                    case 6:
                        this.m02 = value.i;
                        break;
                    case 7:
                        this.m12 = value.i;
                        break;
                    case 8:
                        this.m22 = value.i;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                }
            }
        }

        public override int GetHashCode()
        {
            return this.GetColumn(0).GetHashCode() ^ this.GetColumn(1).GetHashCode() << 2 ^
                   this.GetColumn(2).GetHashCode() >> 2;
        }

        public override bool Equals(object other)
        {
            if (!(other is VMatrix3x3))
                return false;
            return this.Equals((VMatrix3x3)other);
        }

        public bool Equals(VMatrix3x3 other)
        {
            return this.GetColumn(0).Equals(other.GetColumn(0))
                   && this.GetColumn(1).Equals(other.GetColumn(1))
                   && this.GetColumn(2).Equals(other.GetColumn(2));
        }

        public static VMatrix3x3 operator *(VMatrix3x3 lhs, VMatrix3x3 rhs)
        {
            VMatrix3x3 mat;
            mat.m00 = (int)IntMath.Divide(((long)lhs.m00 * (long)rhs.m00 + (long)lhs.m01 * (long)rhs.m10 +
                              (long)lhs.m02 * (long)rhs.m20) ,1000);
            mat.m01 = (int)IntMath.Divide(((long)lhs.m00 * (long)rhs.m01 + (long)lhs.m01 * (long)rhs.m11 +
                              (long)lhs.m02 * (long)rhs.m21), 1000);
            mat.m02 = (int)IntMath.Divide(((long)lhs.m00 * (long)rhs.m02 + (long)lhs.m01 * (long)rhs.m12 +
                              (long)lhs.m02 * (long)rhs.m22), 1000);
            mat.m10 = (int)IntMath.Divide(((long)lhs.m10 * (long)rhs.m00 + (long)lhs.m11 * (long)rhs.m10 +
                              (long)lhs.m12 * (long)rhs.m20), 1000);
            mat.m11 = (int)IntMath.Divide(((long)lhs.m10 * (long)rhs.m01 + (long)lhs.m11 * (long)rhs.m11 +
                              (long)lhs.m12 * (long)rhs.m21), 1000);
            mat.m12 = (int)IntMath.Divide(((long)lhs.m10 * (long)rhs.m02 + (long)lhs.m11 * (long)rhs.m12 +
                              (long)lhs.m12 * (long)rhs.m22), 1000);
            mat.m20 = (int)IntMath.Divide(((long)lhs.m20 * (long)rhs.m00 + (long)lhs.m21 * (long)rhs.m10 +
                              (long)lhs.m22 * (long)rhs.m20), 1000);
            mat.m21 = (int)IntMath.Divide(((long)lhs.m20 * (long)rhs.m01 + (long)lhs.m21 * (long)rhs.m11 +
                              (long)lhs.m22 * (long)rhs.m21), 1000);
            mat.m22 = (int)IntMath.Divide(((long)lhs.m20 * (long)rhs.m02 + (long)lhs.m21 * (long)rhs.m12 +
                              (long)lhs.m22 * (long)rhs.m22), 1000);
            return mat;
        }


        public static bool operator ==(VMatrix3x3 lhs, VMatrix3x3 rhs)
        {
            return lhs.GetColumn(0) == rhs.GetColumn(0) && lhs.GetColumn(1) == rhs.GetColumn(1) &&
                   lhs.GetColumn(2) == rhs.GetColumn(2);
        }

        public static bool operator !=(VMatrix3x3 lhs, VMatrix3x3 rhs)
        {
            return !(lhs == rhs);
        }


        public VInt3 GetColumn(int index)
        {
            switch (index)
            {
                case 0:
                    return new VInt3(this.m00, this.m10, this.m20);
                case 1:
                    return new VInt3(this.m01, this.m11, this.m21);
                case 2:
                    return new VInt3(this.m02, this.m12, this.m22);
                default:
                    throw new IndexOutOfRangeException("Invalid column index!");
            }
        }

        public VInt3 GetRow(int index)
        {
            switch (index)
            {
                case 0:
                    return new VInt3(this.m00, this.m01, this.m02);
                case 1:
                    return new VInt3(this.m10, this.m11, this.m12);
                case 2:
                    return new VInt3(this.m20, this.m21, this.m22);
                default:
                    throw new IndexOutOfRangeException("Invalid row index!");
            }
        }

        public void SetColumn(int index, VInt3 column)
        {
            this[0, index] = column.x;
            this[1, index] = column.y;
            this[2, index] = column.z;
        }

        public void SetRow(int index, VInt3 row)
        {
            this[index, 0] = row.x;
            this[index, 1] = row.y;
            this[index, 2] = row.z;
        }


        private VInt3 MatrixToEuler()
        {
            VInt3 v = new VInt3();
            if (m12 < 1)
            {
                if (m12 > -1)
                {
                    v.x = IntMath.Asin(-m12).i;
                    v.y = IntMath.Atan2(m02, m22).i;
                    v.z = IntMath.Atan2(m10, m11).i;
                }
                else
                {
                    v.x = IntMath.Divide(IntMath.PI.i * VInt.half.i,1000);
                    v.y = IntMath.Atan2(m01, m00).i;
                    v.z = 0;
                }
            }
            else
            {
                v.x = IntMath.Divide( -IntMath.PI.i * VInt.half.i,1000);
                v.y = IntMath.Atan2(-m01, m00).i;
                v.z = 0;
            }

            for (int i = 0; i < 3; i++) // 转换到 [0,2pi]区间
            {
                if (v[i] < 0)
                {
                    v[i] += IntMath.PI2.i;
                }
                else if (v[i] > IntMath.PI2.i)
                {
                    v[i] -= IntMath.PI2.i;
                }
                v[i] = (int)IntMath.Divide((long)v[i] * (long)IntMath.Rad2Deg.i, 1000L);// 转化为角度
            }

            return v;
        }
    }
}