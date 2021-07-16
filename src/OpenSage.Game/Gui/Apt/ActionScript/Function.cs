using System;
using System.Collections.Generic;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    [Flags]
    public enum FunctionPreloadFlags
    {
        PreloadExtern = 0x010000,   //this seems to be added by EA
        PreloadParent = 0x008000,
        PreloadRoot = 0x004000,

        SupressSuper = 0x002000,
        PreloadSuper = 0x001000,
        SupressArguments = 0x000800,
        PreloadArguments = 0x000400,
        SupressThis = 0x000200,
        PreloadThis = 0x000100,
        PreloadGlobal = 0x000001
    }

    public class FunctionArgument
    {
        public int Register;
        public string Parameter;
    }

    public sealed class Function: ObjectContext
    {
        public static Function FunctionConstructor => ObjectContext._ffc;
        public static Function ObjectConstructor => ObjectContext._foc;

        public Function(): base()
        {
            __proto__ = FunctionPrototype;
            var prt = new ObjectContext();
            prt.constructor = this;
            this.prototype = prt;
        }

        internal Function(bool JustUsedToCreateObjectPrototype): base(JustUsedToCreateObjectPrototype)
        {
            __proto__ = FunctionPrototype;
        }

        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public List<Value> Constants { get; set; }
        public ActionContext DefinedContext { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }



        public ActionContext GetContext(VM vm, Value[] args, ObjectContext thisVar)
        {
            var outerVar = vm.CurrentContext();

            var code = Instructions;

            var context = vm.GetActionContext(outerVar, thisVar, NumberRegisters, Constants, code);

            /*var localScope = new ObjectContext(thisVar.Item)
            {
                Constants = Constants,
                Variables = thisVar.Variables
            };

            var context = vm.GetActionContext(NumberRegisters, code, localScope, thisVar.Item.Character.Container.Constants.Entries);
            */
            //new ActionContext()
            //{
            //    Global = GlobalObject,
            //    Scope = localScope,
            //    Apt = scope.Item.Context,
            //    Stream = stream,
            //    Constants = scope.Item.Character.Container.Constants.Entries
            //};

            
            if (!IsNewVersion) // parameters in the old version are just stored as local variables
            {
                for (var i = 0; i < Parameters.Count; ++i)
                {
                    var name = Parameters[i].ToString();
                    bool provided = i < args.Length;
                    context.Params[name] = provided ? args[i] : Value.Undefined();
                }
            }
            else // parameters can be stored in both registers and local variables
            {
                for (var i = 0; i < Parameters.Count; i += 2)
                {
                    var reg = Parameters[i].ToInteger();
                    var name = Parameters[i + 1].ToString();
                    int argIndex = i >> 1;
                    bool provided = (argIndex) < args.Length;

                    if (reg != 0)
                    {
                        context.SetRegister(reg, provided ? args[argIndex] : Value.Undefined());
                    }
                    else
                    {
                        context.Params[name] = provided ? args[argIndex] : Value.Undefined();
                    }
                }
            }

            if (IsNewVersion)
            {
                context.Preload(Flags);
            }

            return context;
        }

        public Value Execute(ActionContext context)
        {
            var stream = context.Stream;

            var instr = stream.GetInstruction();
            InstructionBase prevInstr = null;

            while (instr.Type != InstructionType.End)
            {
                instr.Execute(context);

                if (context.Return)
                    return context.Pop();

                if (stream.IsFinished())
                    break;

                prevInstr = instr;
                instr = stream.GetInstruction();
            }

            return Value.Undefined();
        }
    }
}
