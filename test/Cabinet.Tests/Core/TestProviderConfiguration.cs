﻿using Cabinet.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.Tests.Core {
    public class TestProviderConfiguration : IStorageProviderConfig {
        public TestProviderConfiguration() {
            this.Delimiter = "/";
        }

        public string Delimiter { get; set; }
    }
}
