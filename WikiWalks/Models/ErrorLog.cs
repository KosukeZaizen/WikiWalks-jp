using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace RelatedPages.Models
{
    public class ErrorLog
    {
        public static void InsertErrorLog(string error)
        {
            try
            {
                // StackFrame�N���X���C���X�^���X������
                StackFrame objStackFrame = new StackFrame(1);// �t���[����1�Ȃ璼�ڌĂяo�������\�b�h

                string strClassName = objStackFrame.GetMethod().ReflectedType.FullName;// �Ăяo�����̃N���X�����擾����
                string strMethodName = objStackFrame.GetMethod().Name;// �Ăяo�����̃��\�b�h�����擾����



                var con = new DBCon();

                con.ExecuteUpdate(@"
insert into WikiJpErrorLog 
values (DATEADD(HOUR, 9, GETDATE()), @error);
;",
                    new Dictionary<string, object[]> {
                            { "@error", new object[2] { SqlDbType.NVarChar,
                                $"{strClassName}.{strMethodName}(): {error}"
                            } }
                    });
            }
            catch (Exception ex){ }
        }
    }
}