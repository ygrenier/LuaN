//using LuaLib4Net;
using LuaNet;
using LuaNet.LuaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaProgram
{
    //using LuaApi = LuaNet.LuaLib.Lua;

    /// <summary>
    /// $Id: lua.c,v 1.222 2014/11/11 19:41:27 roberto Exp $
    /// Lua stand-alone interpreter
    /// See Copyright Notice in lua.h
    /// </summary>
    class Program
    {
        const int EXIT_SUCCESS = 0;
        const int EXIT_FAILURE = -1;

        const String LUA_PROMPT = "> ";
        const String LUA_PROMPT2 = ">> ";

        const String LUA_PROGNAME = "lua";

        const int LUA_MAXINPUT = 512;

        const String LUA_INIT_VAR = "LUA_INIT";

        static readonly String LUA_INITVARVERSION;

        static ILuaState NewState() { return new LuaState(); }
///*
//** lua_stdin_is_tty detects whether the standard input is a 'tty' (that
//** is, whether we're running lua interactively).
//*/
//#if !defined(lua_stdin_is_tty)	/* { */

//#if defined(LUA_USE_POSIX)	/* { */

//#include <unistd.h>
//#define lua_stdin_is_tty()	isatty(0)

//#elif defined(LUA_USE_WINDOWS)	/* }{ */

//#include <io.h>
//#define lua_stdin_is_tty()	_isatty(_fileno(stdin))

//#else				/* }{ */

///* ISO C definition */
//#define lua_stdin_is_tty()	1  /* assume stdin is a tty */

//#endif				/* } */

//#endif				/* } */
        static bool lua_stdin_is_tty() { return 0 != (Console.WindowHeight + Console.WindowWidth); }

///*
//** lua_readline defines how to show a prompt and then read a line from
//** the standard input.
//** lua_saveline defines how to "save" a read line in a "history".
//** lua_freeline defines how to free a line read by lua_readline.
//*/
//#if !defined(lua_readline)	/* { */

//#if defined(LUA_USE_READLINE)	/* { */

//#include <readline/readline.h>
//#include <readline/history.h>
//#define lua_readline(L,b,p)	((void)L, ((b)=readline(p)) != NULL)
//#define lua_saveline(L,line)	((void)L, add_history(line))
//#define lua_freeline(L,b)	((void)L, free(b))

//#else				/* }{ */

//#define lua_readline(L,b,p) \
//        ((void)L, fputs(p, stdout), fflush(stdout),  /* show prompt */ \
//        fgets(b, LUA_MAXINPUT, stdin) != NULL)  /* get line */
//#define lua_saveline(L,line)	{ (void)L; (void)line; }
//#define lua_freeline(L,b)	{ (void)L; (void)b; }

//#endif				/* } */

//#endif				/* } */
        static bool lua_readline(ILuaState L, out String b, String p)
        {
            Console.Out.Write(p); Console.Out.Flush();
            b = Console.ReadLine();
            return b != null;
        }
        static void lua_saveline(ILuaState L, String line) { }
        static void lua_freeline(ILuaState L, String b) { }

        static ILuaState globalL = null;

        static String progname = LUA_PROGNAME;

        static Program()
        {
            using (var L = NewState())
            {
                Program.LUA_INITVARVERSION = String.Format("{0}_{1}_{2}", LUA_INIT_VAR, L.LuaVersionMajor, L.LuaVersionMinor);
            }
        }

        /*
        ** Hook set by signal function to stop the interpreter.
        */
        static void lstop(ILuaState L, ILuaDebug ar) {
          //(void)ar;  /* unused arg. */
          L.SetHook(null, 0, 0);  /* reset hook */
          L.Error("interrupted!");
        }


        /*
        ** Function to be called at a C signal. Because a C signal cannot
        ** just change a Lua state (as there is no proper synchronization),
        ** this function only sets a hook that, when called, will stop the
        ** interpreter.
        */
        static void laction(int i)
        {
            //signal(i, SIG_DFL); /* if another SIGINT happens, terminate process */
            globalL.SetHook(lstop, (LuaHookMask.MaskCall | LuaHookMask.MaskRet | LuaHookMask.MaskCount), 1);
        }


        static void print_usage(ILuaState L, String badoption)
        {
            L.WriteStringError("%s: ", progname);
            if (badoption[1] == 'e' || badoption[1] == 'l')
                L.WriteStringError("'%s' needs argument\n", badoption);
            else
                L.WriteStringError("unrecognized option '%s'\n", badoption);
            L.WriteStringError(
            "usage: %s [options] [script [args]]\n"
            + "Available options are:\n"
            + "  -e stat  execute string 'stat'\n"
            + "  -i       enter interactive mode after executing 'script'\n"
            + "  -l name  require library 'name'\n"
            + "  -v       show version information\n"
            + "  -E       ignore environment variables\n"
            + "  --       stop handling options\n"
            + "  -        stop handling options and execute stdin\n"
            ,
            progname);
        }

        /// <summary>
        /// Prints an error message, adding the program name in front of it
        /// (if present)
        /// </summary>
        static void l_message(ILuaState L, String pname, String msg)
        {
            var ol = L;
            L = ol ?? NewState();
            if (!String.IsNullOrEmpty(pname)) L.WriteStringError("%s: ", pname);
            L.WriteStringError("%s\n", msg);
            if (ol == null) L.Dispose();
        }

        /*
        ** Check whether 'status' is not OK and, if so, prints the error
        ** message on the top of the stack. It assumes that the error object
        ** is a string, as it was either generated by Lua or by 'msghandler'.
        */
        static LuaStatus report(ILuaState L, LuaStatus status)
        {
            if (status != LuaStatus.OK)
            {
                String msg = L.ToString(-1);
                l_message(L, progname, msg);
                L.Pop(1);  /* remove message */
            }
            return status;
        }

        /*
        ** Message handler used to run all chunks
        */
        static int msghandler(ILuaState L)
        {
            String msg = L.ToString(1);
            if (msg == null)
            {  /* is error object not a string? */
                if (L.CallMeta(1, "__tostring") /*!= 0*/ &&  /* does it have a metamethod */
                    L.Type(-1) == LuaType.String)  /* that produces a string? */
                    return 1;  /* that is the message */
                else
                    msg = L.PushFString("(error object is a %s value)", L.TypeName(1));
            }
            L.Traceback(L, msg, 1);  /* append a standard traceback */
            return 1;  /* return the traceback */
        }


        /*
        ** Interface to 'lua_pcall', which sets appropriate message function
        ** and C-signal handler. Used to run all chunks.
        */
        static LuaStatus docall(ILuaState L, int narg, int nres)
        {
            LuaStatus status = 0;
            int base_ = L.GetTop() - narg;  /* function index */
            L.PushFunction(msghandler);  /* push message handler */
            L.Insert(base_);  /* put it under function and args */
            globalL = L;  /* to be available to 'laction' */
            Console.CancelKeyPress += Console_CancelKeyPress;
            //signal(SIGINT, laction);  /* set C-signal handler */
            status = L.PCall(narg, nres, base_);
            //signal(SIGINT, SIG_DFL); /* reset C-signal handler */
            Console.CancelKeyPress -= Console_CancelKeyPress;
            L.Remove(base_);  /* remove message handler from the stack */
            return status;
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            laction(0);
            e.Cancel = true;
        }


        static void print_version(ILuaState L)
        {
            L.WriteString(L.LuaCopyright);
            L.WriteLine();
        }


        /// <summary>
        /// Create the 'arg' table, which stores all arguments from the
        /// command line ('argv'). It should be aligned so that, at index 0,
        /// it has 'argv[script]', which is the script name. The arguments
        /// to the script (everything after 'script') go to positive indices;
        /// other arguments (before the script name) go to negative indices.
        /// If there is no script name, assume interpreter's name as base.
        /// </summary>
        static void createargtable(ILuaState L, String[] argv, int argc, int script)
        {
            int i, narg;
            if (script == argc) script = 0;  /* no script name? */
            narg = argc - (script + 1);  /* number of positive indices */
            L.CreateTable(narg, script + 1);
            for (i = 0; i < argc; i++)
            {
                L.PushString(argv[i]);
                L.RawSetI(-2, i - script);
            }
            L.SetGlobal("arg");
        }


        static LuaStatus dochunk(ILuaState L, LuaStatus status)
        {
            if (status == LuaStatus.OK) status = docall(L, 0, 0);
            return report(L, status);
        }


        static LuaStatus dofile(ILuaState L, String name)
        {
            return dochunk(L, L.LoadFile(name));
        }


        static LuaStatus dostring(ILuaState L, String s, String name)
        {
          return dochunk(L, L.LoadBuffer(s, name));
        }


        /*
        ** Calls 'require(name)' and stores the result in a global variable
        ** with the given name.
        */
        static LuaStatus dolibrary(ILuaState L, String name)
        {
            LuaStatus status;
            L.GetGlobal("require");
            L.PushString(name);
            status = docall(L, 1, 1);  /* call 'require(name)' */
            if (status == LuaStatus.OK)
                L.SetGlobal(name);  /* global[name] = require return */
            return report(L, status);
        }


        /*
        ** Returns the string to be used as a prompt by the interpreter.
        */
        static String get_prompt(ILuaState L, bool firstline)
        {
            string p;
            L.GetGlobal(firstline ? "_PROMPT" : "_PROMPT2");
            p = L.ToString(-1);
            if (p == null) p = (firstline ? LUA_PROMPT : LUA_PROMPT2);
            return p;
        }

        /* mark in error messages for incomplete statements */
        const String EOFMARK		="<eof>";
        //const int marklen = (sizeof(EOFMARK) / sizeof(char) - 1);
        static readonly int marklen = EOFMARK.Length;


        /*
        ** Check whether 'status' signals a syntax error and the error
        ** message at the top of the stack ends with the above mark for
        ** incomplete statements.
        */
        static int incomplete(ILuaState L, LuaStatus status)
        {
            if (status == LuaStatus.ErrSyntax)
            {
                int lmsg;
                String msg = L.ToLString(-1, out lmsg);
                //if (lmsg >= marklen && String.Compare(msg + lmsg - marklen, EOFMARK) == 0)
                if (lmsg >= marklen && msg.EndsWith(EOFMARK))
                {
                    L.Pop(1);
                    return 1;
                }
            }
            return 0;  /* else... */
        }


        /*
        ** Prompt the user, read a line, and push it into the Lua stack.
        */
        static int pushline(ILuaState L, bool firstline)
        {
            //char[] buffer = new Char[LUA_MAXINPUT];
            //char *b = buffer;
            string b;
            int l;
            string prmt = get_prompt(L, firstline);
            bool readstatus = lua_readline(L, out b, prmt);
            if (!readstatus)
                return 0;  /* no input (prompt will be popped by caller) */
            L.Pop(1);  /* remove prompt */
            l = b.Length;
            //if (l > 0 && b[l-1] == '\n')  /* line ends with newline? */
            //  b[--l] = '\0';  /* remove it */
            if (firstline && b.StartsWith("="))  /* for compatibility with 5.2, ... */
                L.PushFString("return %s", b.Substring(1));  /* change '=' to 'return' */
            else
                L.PushLString(b, l);
            lua_freeline(L, b);
            return 1;
        }


        /*
        ** Try to compile line on the stack as 'return <line>'; on return, stack
        ** has either compiled chunk or original line (if compilation failed).
        */
        static LuaStatus addreturn(ILuaState L)
        {
            LuaStatus status;
            int len; string line;
            L.PushLiteral("return ");
            L.PushValue(-2);  /* duplicate line */
            L.Concat(2);  /* new line is "return ..." */
            line = L.ToLString(-1, out len);
            if ((status = L.LoadBuffer(line, len, "=stdin")) == LuaStatus.OK)
            {
                L.Remove(-3);  /* remove original line */
                //line += sizeof("return")/sizeof(char);  /* remove 'return' for history */
                line = line.Substring("return".Length);
                if (!String.IsNullOrEmpty(line))  /* non empty? */
                    lua_saveline(L, line);  /* keep history */
            }
            else
                L.Pop(2);  /* remove result from 'luaL_loadbuffer' and new line */
            return status;
        }


        /*
        ** Read multiple lines until a complete Lua statement
        */
        static LuaStatus multiline(ILuaState L)
        {
            for (; ; )
            {  /* repeat until gets a complete statement */
                int len;
                String line = L.ToLString(1, out len);  /* get what it has */
                LuaStatus status = L.LoadBuffer(line, len, "=stdin");  /* try it */
                if (0 == incomplete(L, status) || 0 == pushline(L, false))
                {
                    lua_saveline(L, line);  /* keep history */
                    return status;  /* cannot or should not try to add continuation line */
                }
                L.PushLiteral("\n");  /* add newline... */
                L.Insert(-2);  /* ...between the two lines */
                L.Concat(3);  /* join them */
            }
        }


        /*
        ** Read a line and try to load (compile) it first as an expression (by
        ** adding "return " in front of it) and second as a statement. Return
        ** the final status of load/call with the resulting function (if any)
        ** in the top of the stack.
        */
        static LuaStatus loadline(ILuaState L)
        {
            LuaStatus status;
            L.SetTop(0);
            if (0 == pushline(L, true))
                return (LuaStatus)(-1);  /* no input */
            if ((status = addreturn(L)) != LuaStatus.OK)  /* 'return ...' did not work? */
                status = multiline(L);  /* try as command, maybe with continuation lines */
            L.Remove(1);  /* remove line from the stack */
            L.Assert(L.GetTop() == 1);
            return status;
        }


        /*
        ** Prints (calling the Lua 'print' function) any values on the stack
        */
        static void l_print(ILuaState L)
        {
            int n = L.GetTop();
            if (n > 0)
            {  /* any result to be printed? */
                L.CheckStack(L.MinStack, "too many results to print");
                L.GetGlobal("print");
                L.Insert(1);
                if (L.PCall(n, 0, 0) != LuaStatus.OK)
                    l_message(L, progname, L.PushFString("error calling 'print' (%s)", L.ToString(-1)));
            }
        }


        /*
        ** Do the REPL: repeatedly read (load) a line, evaluate (call) it, and
        ** print any results.
        */
        static void doREPL(ILuaState L)
        {
            LuaStatus status;
            String oldprogname = progname;
            progname = null;  /* no 'progname' on errors in interactive mode */
            while ((status = loadline(L)) != (LuaStatus) (-1))
            {
                if (status == LuaStatus.OK)
                    status = docall(L, 0, L.MultiReturns);
                if (status == LuaStatus.OK) l_print(L);
                else report(L, (LuaStatus)status);
            }
            L.SetTop(0);  /* clear stack */
            L.WriteLine();
            progname = oldprogname;
        }


        /*
        ** Push on the stack the contents of table 'arg' from 1 to #arg
        */
        static int pushargs(ILuaState L)
        {
            int i, n;
            if (L.GetGlobal("arg") != LuaType.Table)
                L.Error("'arg' is not a table");
            n = L.LLen(-1);
            L.CheckStack(n + 3, "too many arguments to script");
            for (i = 1; i <= n; i++)
                L.RawGetI(-i, i);
            L.Remove(-i);  /* remove table from the stack */
            return n;
        }


        static LuaStatus handle_script(ILuaState L, String[] argv, int idx = 0)
        {
            LuaStatus status;
            String fname = argv[idx];
            if (String.Compare(fname, "-") == 0 && String.Compare(argv[idx - 1], "--") != 0)
                fname = null;  /* stdin */
            status = L.LoadFile(fname);
            if (status == LuaStatus.OK)
            {
                int n = pushargs(L);  /* push arguments to script */
                status = docall(L, n, L.MultiReturns);
            }
            return report(L, status);
        }

        /* bits of various argument indicators in 'args' */
        const int has_error = 1;	/* bad option */
        const int has_i = 2;	/* -i */
        const int has_v = 4;	/* -v */
        const int has_e = 8;	/* -e */
        const int has_E = 16;	/* -E */

        /*
        ** Traverses all arguments from 'argv', returning a mask with those
        ** needed before running any Lua code (or an error code if it finds
        ** any invalid argument). 'first' returns the first not-handled argument 
        ** (either the script name or a bad argument in case of error).
        */
        static int collectargs(String[] argv, ref int first)
        {
            argv = (String[])argv.Clone();
            int args = 0, argc = argv.Length;
            int i;
            for (i = 1; i < argc; i++)
            {
                argv[i] = argv[i] + '\0';
                first = i;
                if (argv[i][0] != '-')  /* not an option? */
                    return args;  /* stop handling options */
                switch (argv[i][1])
                {  /* else check option */
                    case '-':  /* '--' */
                        if (argv[i][2] != '\0')  /* extra characters after '--'? */
                            return has_error;  /* invalid option */
                        first = i + 1;
                        return args;
                    case '\0':  /* '-' */
                        return args;  /* script "name" is '-' */
                    case 'E':
                        if (argv[i][2] != '\0')  /* extra characters after 1st? */
                            return has_error;  /* invalid option */
                        args |= has_E;
                        break;
                    case 'i':
                        args |= has_i;  /* (-i implies -v) *//* FALLTHROUGH */
                        goto case 'v';
                    case 'v':
                        if (argv[i][2] != '\0')  /* extra characters after 1st? */
                            return has_error;  /* invalid option */
                        args |= has_v;
                        break;
                    case 'e':
                        args |= has_e;  /* FALLTHROUGH */
                        goto case 'l';
                    case 'l':  /* both options need an argument */
                        if (argv[i][2] == '\0')
                        {  /* no concatenated argument? */
                            i++;  /* try next 'argv' */
                            if (argv[i] == null || argv[i][0] == '-')
                                return has_error;  /* no next argument or it is another option */
                        }
                        break;
                    default:  /* invalid option */
                        return has_error;
                }
            }
            first = i;  /* no script name */
            return args;
        }


        /*
        ** Processes options 'e' and 'l', which involve running Lua code.
        ** Returns 0 if some code raises an error.
        */
        static int runargs(ILuaState L, String[] argv, int n)
        {
            int i;
            for (i = 1; i < n; i++)
            {
                int option = argv[i][1];
                L.Assert(argv[i][0] == '-');  /* already checked */
                if (option == 'e' || option == 'l')
                {
                    LuaStatus status;
                    String extra = argv[i].Substring(2);  /* both options need an argument */
                    if (String.IsNullOrEmpty(extra)) extra = argv[++i];
                    L.Assert(extra != null);
                    status = (option == 'e')
                             ? dostring(L, extra, "=(command line)")
                             : dolibrary(L, extra);
                    if (status != LuaStatus.OK) return 0;
                }
            }
            return 1;
        }


        static LuaStatus handle_luainit(ILuaState L)
        {
            //String name = "=" + LUA_INITVARVERSION;
            //String init = getenv(name + 1);
            //if (init == null)
            //{
            //    name = "=" + LUA_INIT_VAR;
            //    init = getenv(name + 1);  /* try alternative name */
            //}
            //if (init == null) return LUA_OK;
            //else if (init[0] == '@')
            //    return dofile(L, init + 1);
            //else
            //    return dostring(L, init, name);
            String name = "=" + LUA_INITVARVERSION;
            String init = Environment.GetEnvironmentVariable(name.Substring(1));
            if (init == null)
            {
                name = "=" + LUA_INIT_VAR;
                init = Environment.GetEnvironmentVariable(name.Substring(1));   /* try alternative name */
            }
            if (init == null) return LuaStatus.OK;
            else if (init.StartsWith("@"))
                return dofile(L, init.Substring(1));
            else
                return dostring(L, init, name);
        }

        /// <summary>
        /// Main body of stand-alone interpreter (to be called in protected mode).
        /// Reads the options and handles them all.
        /// </summary>
        static int pmain(ILuaState L)
        {
            int argc = L.ToInteger(1);
            var argv = L.ToUserData<String[]>(2);
            int script = 0;
            int args = collectargs(argv, ref script);
            L.CheckVersion();   /* check that interpreter has correct version */
            if (argc > 0 && !String.IsNullOrWhiteSpace(argv[0])) progname = argv[0];
            if (args == has_error)
            {  /* bad arg? */
                print_usage(L, argv[script]);  /* 'script' has index of bad arg. */
                return 0;
            }
            if ((args & has_v) != 0)  /* option '-v'? */
                print_version(L);
            if ((args & has_E) != 0)
            {  /* option '-E'? */
                L.PushBoolean(true);  /* signal for libraries to ignore env. vars. */
                L.SetField(L.RegistryIndex, "LUA_NOENV");
            }
            //  luaL_openlibs(L);  /* open standard libraries */
            L.OpenLibs();
            createargtable(L, argv, argc, script);  /* create table 'arg' */
            if (0 == (args & has_E))
            {  /* no option '-E'? */
                if (handle_luainit(L) != LuaStatus.OK)  /* run LUA_INIT */
                    return 0;  /* error running LUA_INIT */
            }
            if (runargs(L, argv, script) == 0)  /* execute arguments -e and -l */
                return 0;  /* something failed */
            if (script < argc &&  /* execute main script (if there is one) */
                handle_script(L, argv, script) != LuaStatus.OK)
                return 0;
            if ((args & has_i) != 0)  /* -i option? */
                doREPL(L);  /* do read-eval-print loop */
            else if (script == argc && 0 == (args & (has_e | has_v)))
            {  /* no arguments? */
                if (lua_stdin_is_tty())
                {  /* running in interactive mode? */
                    print_version(L);
                    doREPL(L);  /* do read-eval-print loop */
                }
                else dofile(L, null);  /* executes stdin as a file */
            }
            L.PushBoolean(true);  /* signal no errors */
            return 1;
        }


        static int Main(string[] args)
        {
            args = Environment.GetCommandLineArgs();
            int argc = args.Length;
            String[] argv = args;

            LuaStatus status; bool result;
            try
            {
                using (var L = NewState())
                { 
                    L.PushFunction(pmain); /* to call 'pmain' in protected mode */
                    L.PushInteger(argc);    /* 1st argument */
                    L.PushLightUserData(argv); /* 2nd argument */
                    status = L.PCall(2, 1, 0);  /* do the call */
                    result = L.ToBoolean(-1);  /* get result */
                    report(L, status);
                }
            }
            catch (Exception ex)
            {
                l_message(null, args[0], String.Format("cannot create state: {0}", ex.GetBaseException().Message));
                result = false;
                status = LuaStatus.ErrRun;
            }
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("\nPress a key to quit...");
                Console.Read();
            }
#endif
            return (result && status == LuaStatus.OK) ? EXIT_SUCCESS : EXIT_FAILURE;
        }

    }
}
