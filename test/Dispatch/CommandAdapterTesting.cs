﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fidget.Commander.Dispatch
{
    /// <summary>
    /// Tests of the command adapter type.
    /// </summary>
    
    public class CommandAdapterTesting
    {
        /// <summary>
        /// Command type for testing.
        /// </summary>
        
        public class TestCommand : ICommand<object> {}

        /// <summary>
        /// Mock command handler.
        /// </summary>
        
        protected Mock<ICommandHandler<TestCommand,object>> MockHandler = new Mock<ICommandHandler<TestCommand, object>>();

        /// <summary>
        /// Creates and returns an instance using the configured arguments.
        /// </summary>
        
        protected ICommandAdapter<object> CreateInstance() => new CommandAdapter<TestCommand,object>( MockHandler?.Object );
        
        /// <summary>
        /// Tests of the constructor method.
        /// </summary>
        
        public class Constructor : CommandAdapterTesting
        {
            /// <summary>
            /// Handler argument should be required.
            /// </summary>
            
            [Fact]
            public void Requires_Handler()
            {
                MockHandler = null;
                Assert.Throws<ArgumentNullException>( "handler", () => CreateInstance() );
            }
        }

        /// <summary>
        /// Tests of the execute method.
        /// </summary>
        
        public class Execute : CommandAdapterTesting
        {
            /// <summary>
            /// Command argument.
            /// </summary>
            
            ICommand<object> Command = new TestCommand();

            /// <summary>
            /// Cancellation token argument.
            /// </summary>
            
            CancellationToken CancellationToken = CancellationToken.None;

            /// <summary>
            /// Calls the execute method with the configured values.
            /// </summary>
            
            Task<object> CallExecute() => CreateInstance().Execute( Command, CancellationToken );

            /// <summary>
            /// Command argument should be required.
            /// </summary>
            
            [Fact]
            public async Task Requires_Command()
            {
                Command = null;
                await Assert.ThrowsAsync<ArgumentNullException>( "command", CallExecute );
            }

            /// <summary>
            /// Command type matching the expected interface.
            /// </summary>
            
            class WrongCommand : ICommand<object> {}

            /// <summary>
            /// Command argument should be verified to be the expected concrete type.
            /// </summary>
            
            [Fact]
            public async Task Verifies_CommandIsExpectedType()
            {
                Command = new WrongCommand();
                await Assert.ThrowsAsync<ArgumentException>( "command", CallExecute );
            }

            /// <summary>
            /// Execution should be interrupted when cancelled prior to calling the handler.
            /// </summary>
            
            [Fact]
            public async Task Interrupts_WhenCancelled()
            {
                CancellationToken = new CancellationToken( true );
                await Assert.ThrowsAsync<OperationCanceledException>( CallExecute );
            }

            /// <summary>
            /// Return value should match the return value of the handler.
            /// </summary>
            
            [Fact]
            public async Task Returns_HandlerResult()
            {
                var expected = new object();
                var command = (TestCommand)Command;
                MockHandler.Setup( _=> _.Handle( command, CancellationToken ) ).ReturnsAsync( expected );
                
                var actual = await CallExecute();
                Assert.Equal( expected, actual );

                MockHandler.Verify( _=> _.Handle( command, CancellationToken ), Times.Once );
            }
        }
    }
}